using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.General;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;
using AssemblyCSharp.Assets.Code.Core.Screen.Interface;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Zenject;
using AssemblyCSharp.Assets.Code.Core.YGOProDeck.Impl;

namespace AssemblyCSharp.Assets.Code.Features.SpeedDuel
{
    public class SpeedDuelView : MonoBehaviour, ISmartDuelEventListener
    {
        private static readonly string Card_Back = "CardBack";
        
        [SerializeField]
        private GameObject _objectToPlace;
        [SerializeField]
        private GameObject _placementIndicator;
        [SerializeField]
        private GameObject _particles;

        private ISmartDuelServer _smartDuelServer;
        private IDataManager _dataManager;

        private ApiWebRequest _webRequest;
        private ARRaycastManager _arRaycastManager;
        private ARPlaneManager _arPlaneManager;
        private List<ARRaycastHit> _hits;
        private Pose _placementPose;
        private bool _placementPoseIsValid = false;
        private bool _objectPlaced = false;

        #region Properties

        private GameObject SpeedDuelField { get; set; }
        private Dictionary<string, GameObject> InstantiatedModels { get; } = new Dictionary<string, GameObject>();

        #endregion

        #region Constructors

        [Inject]
        public void Construct(
            ISmartDuelServer smartDuelServer,
            IDataManager dataManager,
            IScreenService screenService)
        {
            _smartDuelServer = smartDuelServer;
            _dataManager = dataManager;

            screenService.UseAutoOrientation();
            ConnectToServer();
        }

        #endregion

        #region Lifecycle

        private void Awake()
        {
            GetObjectReferences();
<<<<<<< Updated upstream
=======
        }

        private void Start()
        {
            _dataManager.CreateRecycler();
            InstantiateObjectPool("Particles", (int)RecyclerKeys.DestructionParticles, _particles, 6);
            InstantiateObjectPool("SetCards", (int)RecyclerKeys.SetCard, _dataManager.GetCardModel(SET_CARD), 6);
>>>>>>> Stashed changes
        }

        private void Update()
        {
            UpdatePlacementIndicatorIfNecessary();
        }

        private void OnDestroy()
        {
            _smartDuelServer?.Dispose();
        }

        #endregion

        #region Placement indicator

        private void GetObjectReferences()
        {
            _arRaycastManager = FindObjectOfType<ARRaycastManager>();
            _arPlaneManager = FindObjectOfType<ARPlaneManager>();
        }

        private void UpdatePlacementIndicatorIfNecessary()
        {
            if (_objectPlaced)
            {
                return;
            }

            _hits = UpdatePlacementPose();
            UpdatePlacementIndicator();

            if (_placementPoseIsValid && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                PlaceObject();
                SetPendulumScale(_hits);
            }
        }

        private List<ARRaycastHit> UpdatePlacementPose()
        {
            var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
            var hits = new List<ARRaycastHit>();
            _arRaycastManager.Raycast(screenCenter, hits, TrackableType.Planes);

            _placementPoseIsValid = hits.Count > 0;
            if (_placementPoseIsValid)
            {
                _placementPose = hits[hits.Count-1].pose;

                var cameraForward = Camera.current.transform.forward;
                var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
                _placementPose.rotation = Quaternion.LookRotation(cameraBearing);
            }

            return hits;
        }

        private void UpdatePlacementIndicator()
        {
            if (_placementPoseIsValid)
            {
                _placementIndicator.SetActive(true);
                _placementIndicator.transform.SetPositionAndRotation(_placementPose.position, _placementPose.rotation);
            }
            else
            {
                _placementIndicator.SetActive(false);
            }
        }

        private void PlaceObject()
        {
            _objectPlaced = true;
            _placementIndicator.SetActive(false);
            SpeedDuelField = Instantiate(_objectToPlace, _placementPose.position, _placementPose.rotation);
        }

        private void SetPendulumScale(List<ARRaycastHit> hits)
        {
            var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
            _arRaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinBounds);

            if (hits == null)
            {
                return;
            }

            var scalePlane = GetCameraOrientation(_arPlaneManager.GetPlane(hits[hits.Count-1].trackableId));

            if (scalePlane <= 0)
            {
                return;
            }

            SpeedDuelField.transform.localScale = new Vector3(scalePlane, scalePlane, scalePlane);

            _arPlaneManager.SetTrackablesActive(false);
            _arPlaneManager.enabled = false;
        }

        private float GetCameraOrientation(ARPlane plane)
        {
            float scaleAmount;
            var cameraOriantation = Camera.current.transform.rotation.y;

            if (cameraOriantation.IsWithin(45, 135)   || 
                cameraOriantation.IsWithin(225, 315)  ||
                cameraOriantation.IsWithin(-45, -135) || 
                cameraOriantation.IsWithin(-225, -315))
            {
                scaleAmount = plane.size.y;
            }
            else
            {
                scaleAmount = plane.size.x;
            }

            return scaleAmount;
        }

        private void OnPlaymatDestroyed()
        {
            _objectPlaced = false;
            _placementIndicator.SetActive(true);
            _arPlaneManager.enabled = true;
        }

        #endregion

        #region Smart duel events

        private void ConnectToServer()
        {
            _smartDuelServer.Connect(this);
        }

        public void onSmartDuelEventReceived(SmartDuelEvent smartDuelEvent)
        {
            if (smartDuelEvent is SummonEvent summonEvent)
            {
                OnSummonEventReceived(summonEvent);
            }
            else if (smartDuelEvent is RemoveCardEvent removeCardEvent)
            {
                OnRemovecardEventReceived(removeCardEvent);
            }
            else if (smartDuelEvent is PositionChangeEvent positionChangeEvent)
            {
                OnPositionChangeEventRecieved(positionChangeEvent);
            }
        }

        private void OnSummonEventReceived(SummonEvent summonEvent)
        {
            var zone = SpeedDuelField.transform.Find("Playmat/" + summonEvent.ZoneName);
            if (zone == null)
            {
                return;
            }

            var cardModel = _dataManager.GetCardModel(summonEvent.CardId);
            if (cardModel == null)
            {
                return;
            }

            var instantiatedModel = Instantiate(cardModel, zone.transform.position, zone.transform.rotation, SpeedDuelField.transform);

            var animator = instantiatedModel.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.SetTrigger(AnimatorIDSetter.Animator_Summoning_Trigger);
            }

            InstantiatedModels.Add(summonEvent.ZoneName, instantiatedModel);

            bool isSet = summonEvent.IsSet;
            if (isSet)
            {
<<<<<<< Updated upstream
                var cardBack = _dataManager.GetCardModel(Card_Back);
                if (cardBack == null)
=======
                _webRequest.GetImageFromAPI(cardModel.name);

                var setCardImage = _dataManager.GetCardModel(SET_CARD);
                if (setCardImage == null)
>>>>>>> Stashed changes
                {
                    return;
                }

                var characterMesh = instantiatedModel.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (SkinnedMeshRenderer item in characterMesh)
                {
                    item.enabled = false;
                }

<<<<<<< Updated upstream
                var setCardBack = Instantiate(cardBack, zone.transform.position, zone.transform.rotation, SpeedDuelField.transform);
                InstantiatedModels.Add(summonEvent.ZoneName + "SetCard", setCardBack);
=======
                var setCardModel = _dataManager.UseFromQueue((int)RecyclerKeys.SetCard, zone.position, zone.rotation, SpeedDuelField.transform);
                if (!_dataManager.CheckForCachedImage(cardModel.name))
                {
                    Debug.LogError("No Cached Image");
                    return;
                }
                var imageSetter = setCardModel.GetComponentInChildren<IImageSetter>();
                imageSetter.ChangeImageFromAPI(cardModel.name);

                InstantiatedModels.Add(summonEvent.ZoneName + "SetCard", setCardModel);
>>>>>>> Stashed changes
            }
        }

        private void OnRemovecardEventReceived(RemoveCardEvent removeCardEvent)
        {
            var modelExists = InstantiatedModels.TryGetValue(removeCardEvent.ZoneName, out var model);
            if (!modelExists)
            {
                return;
            }

            var characterMesh = model.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer item in characterMesh)
            {
                item.enabled = false;
            }

            var meshToDestroy = _particles.GetComponent<ISetMeshCharacter>();
            meshToDestroy.GetCharacterMesh(characterMesh[0]);
            var destructionParticles = Instantiate(_particles, SpeedDuelField.transform);

            Destroy(model, 10f);
            Destroy(destructionParticles, 10f);
            InstantiatedModels.Remove(removeCardEvent.ZoneName);

            var modelIsSet = InstantiatedModels.TryGetValue(removeCardEvent.ZoneName + "SetCard", out var setCardModel);
            if (!modelIsSet)
            {
                return;
            }
            InstantiatedModels.Remove(removeCardEvent.ZoneName + "SetCard");
<<<<<<< Updated upstream
            Destroy(setCardBack);
=======
            _dataManager.AddToQueue((int)RecyclerKeys.SetCard, setCardModel);
>>>>>>> Stashed changes
        }

        private void OnPositionChangeEventRecieved(PositionChangeEvent positionChangeEvent)
        {
            var zone = SpeedDuelField.transform.Find("Playmat/" + positionChangeEvent.ZoneName);

            var modelExists = InstantiatedModels.TryGetValue(positionChangeEvent.ZoneName, out var model);
            if (!modelExists)
            {
                return;
            }

            if (!positionChangeEvent.IsSet)
            {
                var characterMesh = model.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (SkinnedMeshRenderer item in characterMesh)
                {
                    item.enabled = true;
                }
                return;
            }

            var cardBackExists = InstantiatedModels.TryGetValue(positionChangeEvent.ZoneName + "SetCard", out var setCardBack);
            if (!cardBackExists)
            {
                var cardBack = _dataManager.GetCardModel(Card_Back);
                if (cardBack == null)
                {
                    return;
                }

                var characterMesh = model.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (SkinnedMeshRenderer item in characterMesh)
                {
                    item.enabled = false;
                }

                var setCardBackModel = Instantiate(cardBack, zone.transform.position, zone.transform.rotation, SpeedDuelField.transform);
                InstantiatedModels.Add(positionChangeEvent.ZoneName + "SetCard", setCardBackModel);
            }
            else
            {
                var characterMesh = model.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (SkinnedMeshRenderer item in characterMesh)
                {
                    item.enabled = false;
                }
            }
        }
        #endregion
    }
}
