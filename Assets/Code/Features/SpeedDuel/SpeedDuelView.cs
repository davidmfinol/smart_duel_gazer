using System;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.General;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;
using AssemblyCSharp.Assets.Code.Core.Screen.Interface;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.ModelRecycler.Entities;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Zenject;

namespace AssemblyCSharp.Assets.Code.Features.SpeedDuel
{
    public class SpeedDuelView : MonoBehaviour, ISmartDuelEventListener
    {
        private static readonly string SET_CARD = "SetCard";
        private static readonly string PLAYMAT_ZONES = "Playmat/Zones/";

        [SerializeField]
        private GameObject _objectToPlace;
        [SerializeField]
        private GameObject _placementIndicator;
        [SerializeField]
        private GameObject _particles;

        private ISmartDuelServer _smartDuelServer;
        private IDataManager _dataManager;

        private ARRaycastManager _arRaycastManager;
        private ARPlaneManager _arPlaneManager;
        private List<ARRaycastHit> _hits;
        private List<GameObject> _inactiveModels;
        private Pose _placementPose;
        private bool _placementPoseIsValid = false;
        private bool _objectPlaced = false;
        
        private GameObject _testModel;

        #region Properties

        private GameObject SpeedDuelField { get; set; }
        private Dictionary<string, GameObject> InstantiatedModels { get; } = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> InactiveModels { get; } = new Dictionary<string, GameObject>();

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
            _inactiveModels = new List<GameObject>();
        }

        private void Start()
        {
            _dataManager.CreateRecycler();
            InstantiateObjectPool("Particles", (int)RecyclerKeys.DestructionParticles, _particles, 6);
            InstantiateObjectPool("SetCards", (int)RecyclerKeys.SetCard, _dataManager.GetCardModel(SET_CARD), 6);
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

        private void InstantiateObjectPool(string parentName, int key, GameObject prefab, int amount)
        {
            var parent = new GameObject(parentName + " Pool");
            
            for (int i = 0; i < amount; i++)
            {
                var obj = Instantiate(prefab, parent.transform);
                _dataManager.AddToQueue(key, obj);
            }
        }

        private void UpdatePlacementIndicatorIfNecessary()
        {

#if UNITY_EDITOR
            if (_objectPlaced && Input.GetKeyDown(KeyCode.F))
            {
                _testModel = FlipRandomImage();
            }

            if (_objectPlaced && Input.GetKeyDown(KeyCode.G))
            {
                ReturnToFaceDown(_testModel);
            }

            if (!_objectPlaced && Input.GetKeyDown(KeyCode.Space))
            {
                PlaceObject();
            }

            return;
#endif

#pragma warning disable CS0162 // Unreachable code detected
            if (_objectPlaced)
#pragma warning restore CS0162 // Unreachable code detected
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

            var scalePlane = GetCameraOrientation(_arPlaneManager.GetPlane(hits[hits.Count].trackableId));

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

            if (cameraOriantation.IsWithinRange(45, 135)   || 
                cameraOriantation.IsWithinRange(225, 315)  ||
                cameraOriantation.IsWithinRange(-45, -135) || 
                cameraOriantation.IsWithinRange(-225, -315))
            {
                scaleAmount = plane.size.y;
            }
            else
            {
                scaleAmount = plane.size.x;
            }

            return scaleAmount;
        }

        private GameObject FlipRandomImage()
        {
            bool exists = InstantiatedModels.TryGetValue("mainMonster3SetCard", out var model);
            if(!exists)
            {
                return null;
            }

            var image = model.GetComponentInChildren<IImageSetter>();
            image.ChangeImage();
            model.transform.rotation = Quaternion.Euler(0, 90, 90);

            return model;
        }

        private void ReturnToFaceDown(GameObject model)
        {
            if(model != null)
            {
                model.transform.rotation = Quaternion.Euler(0, 0, 0);
            }            
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
            var zone = SpeedDuelField.transform.Find(PLAYMAT_ZONES + summonEvent.ZoneName);
            if (zone == null)
            {
                return;
            }

            var cardModel = _dataManager.GetCardModel(summonEvent.CardId);
            if (cardModel == null)
            {
                return;
            }

            GameObject instantiatedModel;
            if (_dataManager.CheckForExistingModel(cardModel.name + "(Clone)"))
            {
                instantiatedModel = _dataManager.GetExistingModel(cardModel.name + "(Clone)", SpeedDuelField.transform);
                instantiatedModel.transform.SetPositionAndRotation(zone.position, zone.rotation);
            }
            else
            {
                instantiatedModel = Instantiate(cardModel, zone.transform.position, zone.transform.rotation, SpeedDuelField.transform);
            }
            
            var animator = instantiatedModel.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.SetTrigger(AnimatorIDSetter.Animator_Summoning_Trigger);
            }

            InstantiatedModels.Add(summonEvent.ZoneName, instantiatedModel);

            if (summonEvent.IsSet)
            {
                var setCardImage = _dataManager.GetCardModel(SET_CARD);
                if (setCardImage == null)
                {
                    return;
                }

                var characterMesh = _dataManager.GetMeshRenderers(instantiatedModel.name, instantiatedModel);
                characterMesh.SetRendererVisibility(false);                

                var setCardBack = _dataManager.UseFromQueue((int)RecyclerKeys.SetCard, zone.position, zone.rotation, SpeedDuelField.transform);
                InstantiatedModels.Add(summonEvent.ZoneName + "SetCard", setCardBack);
            }
        }        

        private void OnRemovecardEventReceived(RemoveCardEvent removeCardEvent)
        {
            var zone = SpeedDuelField.transform.Find(PLAYMAT_ZONES + removeCardEvent.ZoneName);

            var modelExists = InstantiatedModels.TryGetValue(removeCardEvent.ZoneName, out var model);
            if (!modelExists)
            {
                return;
            }

            var characterMesh = _dataManager.GetMeshRenderers(model.name, model);
            characterMesh.SetRendererVisibility(false);

            var destructionParticles = _dataManager.UseFromQueue((int)RecyclerKeys.DestructionParticles, 
                                                                 zone.transform.position,
                                                                 zone.transform.rotation,
                                                                 SpeedDuelField.transform,
                                                                 characterMesh[0]);

            StartCoroutine(WaitToProceed((int)RecyclerKeys.DestructionParticles, destructionParticles, 10f));
            StartCoroutine(WaitToProceed(model.name, model, 10f));

            InstantiatedModels.Remove(removeCardEvent.ZoneName);

            var modelIsSet = InstantiatedModels.TryGetValue(removeCardEvent.ZoneName + "SetCard", out var setCardBack);
            if (!modelIsSet)
            {
                return;
            }
            InstantiatedModels.Remove(removeCardEvent.ZoneName + "SetCard");
            _dataManager.AddToQueue((int)RecyclerKeys.SetCard, setCardBack);
        }

        private void OnPositionChangeEventRecieved(PositionChangeEvent positionChangeEvent)
        {
            var zone = SpeedDuelField.transform.Find(PLAYMAT_ZONES + positionChangeEvent.ZoneName);

            var modelExists = InstantiatedModels.TryGetValue(positionChangeEvent.ZoneName, out var model);
            if (!modelExists)
            {
                return;
            }

            if (!positionChangeEvent.IsSet)
            {
                var characterMesh = _dataManager.GetMeshRenderers(model.name, model);
                characterMesh.SetRendererVisibility(true);
                return;
            }

            var cardBackExists = InstantiatedModels.TryGetValue(positionChangeEvent.ZoneName + "SetCard", out var _);
            if (!cardBackExists)
            {
                var cardBack = _dataManager.GetCardModel(SET_CARD);
                if (cardBack == null)
                {
                    return;
                }

                var characterMesh = _dataManager.GetMeshRenderers(model.name, model);
                characterMesh.SetRendererVisibility(false);

                var setCardBackModel = _dataManager.UseFromQueue((int)RecyclerKeys.SetCard, zone.position, zone.rotation, SpeedDuelField.transform);
                InstantiatedModels.Add(positionChangeEvent.ZoneName + "SetCard", setCardBackModel);
            }
            else
            {
                var characterMesh = _dataManager.GetMeshRenderers(model.name, model);
                characterMesh.SetRendererVisibility(false);
            }
        }

        #endregion

        #region Coroutines

        private IEnumerator WaitToProceed(int key, GameObject model, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            _dataManager.AddToQueue(key, model);
        }
        private IEnumerator WaitToProceed(string key, GameObject model, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            _dataManager.RecycleModel(key, model);
        }

        #endregion
    }
}
