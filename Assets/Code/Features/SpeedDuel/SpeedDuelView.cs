using Zenject;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp.Assets.Code.Core.Screen.Interface;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.ModelRecycler.Entities;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler;
using AssemblyCSharp.Assets.Code.Core.Models.Interface.ModelEventsHandler.Entities;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelComponentsManager;
using AssemblyCSharp.Assets.Code.Core.YGOProDeck.Impl;
using AssemblyCSharp.Assets.Code.Core.General;
//Add in API using statements

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
        private ModelEventHandler _eventHandler;
        private ModelFactory _modelFactory;
        private ParticleFactory _particleFactory;

        private ApiWebRequest _webRequest;
        private ARRaycastManager _arRaycastManager;
        private ARPlaneManager _arPlaneManager;
        private List<ARRaycastHit> _hits;
        private Pose _placementPose;
        private WaitForSeconds _waitTime = new WaitForSeconds(10);
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
            IScreenService screenService,
            ModelEventHandler modelEventHandler,
            ModelFactory modelFactory,
            ParticleFactory particleFactory)
        {
            _smartDuelServer = smartDuelServer;
            _dataManager = dataManager;
            _eventHandler = modelEventHandler;
            _modelFactory = modelFactory;
            _particleFactory = particleFactory;

            screenService.UseAutoOrientation();
            ConnectToServer();
        }

        #endregion

        #region Lifecycle

        private void Awake()
        {
            GetObjectReferences();
        }

        private void Start()
        {
            _dataManager.CreateRecycler();
            BuildPrefabFromFactory("Particles", (int)RecyclerKeys.DestructionParticles, _particles, 6);
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
            _webRequest = GetComponent<ApiWebRequest>();
        }

        //This function has been refactored for the next update
        private void InstantiateObjectPool(string parentName, int key, GameObject prefab, int amount)
        {
            var parent = new GameObject(parentName + " Pool");

            for (int i = 0; i < amount; i++)
            {
                var obj = Instantiate(prefab, parent.transform);
                _dataManager.AddToQueue(key, obj);
            }
        }

        private void BuildPrefabFromFactory(string parentName, int key, GameObject prefab, int amount)
        {
            var parent = new GameObject(parentName + " Pool");

            for (int i = 0; i < amount; i++)
            {
                var obj = _particleFactory.Create(prefab).gameObject;
                obj.transform.SetParent(parent.transform);
                _dataManager.AddToQueue(key, obj);
            }
        }

        private void UpdatePlacementIndicatorIfNecessary()
        {

#if UNITY_EDITOR
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
                SetPlaymatScale(_hits);
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

        private void SetPlaymatScale(List<ARRaycastHit> hits)
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
            var cameraOriantation = Camera.current.transform.rotation.y;

            if (cameraOriantation.IsWithinRange(45, 135)   || 
                cameraOriantation.IsWithinRange(225, 315)  ||
                cameraOriantation.IsWithinRange(-45, -135) || 
                cameraOriantation.IsWithinRange(-225, -315))
            {
                return plane.size.y;
            }
            
            return plane.size.x;
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
            if (smartDuelEvent is SummonCardEvent summonEvent)
            {
                OnSummonEventReceived(summonEvent);
            }
            else if (smartDuelEvent is RemoveCardEvent removeCardEvent)
            {
                OnRemovecardEventReceived(removeCardEvent);
            }
        }

        //Add logic for spell/trap events
        private void OnSummonEventReceived(SummonCardEvent summonEvent)
        {
            var zone = SpeedDuelField.transform.Find(PLAYMAT_ZONES + summonEvent.ZoneName);
            if (zone == null)
            {
                return;
            }

            if (!InstantiatedModels.ContainsKey(summonEvent.ZoneName))
            {
                var cardModel = _dataManager.GetCardModel(summonEvent.CardId);
                if (cardModel == null)
                {
                    return;
                }

                GameObject instantiatedModel;
                if (_dataManager.CheckForExistingModel(cardModel.name))
                {
                    instantiatedModel = _dataManager.UseFromQueue(cardModel.name, SpeedDuelField.transform);
                    instantiatedModel.transform.SetPositionAndRotation(zone.position, zone.rotation);
                }
                else
                {
                    instantiatedModel = _modelFactory.Create(cardModel).gameObject.transform.parent.gameObject;
                    instantiatedModel.transform.SetParent(SpeedDuelField.transform);
                    instantiatedModel.transform.SetPositionAndRotation(zone.position, zone.rotation);
                }

                _eventHandler.RaiseEvent(EventNames.SummonMonster, summonEvent.ZoneName);
                InstantiatedModels.Add(summonEvent.ZoneName, instantiatedModel);
            }

            if (summonEvent.CardPosition == "faceDownDefence")
            {
                Debug.Log(summonEvent.CardPosition, this);

                var setCardImage = _dataManager.GetCardModel(SET_CARD);
                if (setCardImage == null)
                {
                    return;
                }

                _eventHandler.RaiseEvent(EventNames.ChangeMonsterVisibility, summonEvent.ZoneName, false);

                var setCardBack = _dataManager.UseFromQueue((int)RecyclerKeys.SetCard, zone.position, zone.rotation, SpeedDuelField.transform);
                InstantiatedModels.Add(summonEvent.ZoneName + SET_CARD, setCardBack);
            }
            else if (summonEvent.CardPosition == "faceUpDefence")
            {
                //Add animations & events for set cards with API update
                Debug.Log(summonEvent.CardPosition, this);

                var setCardImage = _dataManager.GetCardModel(SET_CARD);
                if (setCardImage == null)
                {
                    return;
                }

                _eventHandler.RaiseEvent(EventNames.ChangeMonsterVisibility, summonEvent.ZoneName, true);

                var setCardModel = _dataManager.UseFromQueue((int)RecyclerKeys.SetCard, zone.position, zone.rotation, SpeedDuelField.transform);
                if (!_dataManager.CheckForCachedImage(summonEvent.CardId))
                {
                    Debug.LogError("No Cached Image");
                    return;
                }
                var imageSetter = setCardModel.GetComponentInChildren<IImageSetter>();
                imageSetter.ChangeImageFromAPI(summonEvent.CardId);

                InstantiatedModels.Add(summonEvent.ZoneName + SET_CARD, setCardModel);
            }

            //Add spell/trap faceUp, faceDown logic with API update
        }

        //Ensure the steps aren't duplicated
        private void OnRemovecardEventReceived(RemoveCardEvent removeCardEvent)
        {
            var modelExists = InstantiatedModels.TryGetValue(removeCardEvent.ZoneName, out var model);
            if (!modelExists)
            {
                var modelSet = InstantiatedModels.TryGetValue(removeCardEvent.ZoneName + "SetCard", out var setCardBack);
                if (!modelSet)
                {
                    return;
                }
                var animator = setCardBack.GetComponent<Animator>();
                animator.SetTrigger(AnimatorIDSetter.Animator_Remove_Spell_Or_Trap);

                InstantiatedModels.Remove(removeCardEvent.ZoneName + "SetCard");
                StartCoroutine(WaitToProceed((int)RecyclerKeys.SetCard, setCardBack));

                return;
            }

            var destructionParticles = _dataManager.UseFromQueue(
                (int)RecyclerKeys.DestructionParticles, SpeedDuelField.transform);
            
            _eventHandler.RaiseEvent(EventNames.DestroyMonster, removeCardEvent.ZoneName, false);

            StartCoroutine(WaitToProceed((int)RecyclerKeys.DestructionParticles, destructionParticles));
            StartCoroutine(WaitToProceed(model.name, model));

            InstantiatedModels.Remove(removeCardEvent.ZoneName);

            var modelIsSet = InstantiatedModels.TryGetValue(removeCardEvent.ZoneName + SET_CARD, out var setCard);
            if (!modelIsSet)
            {
                return;
            }
            InstantiatedModels.Remove(removeCardEvent.ZoneName + SET_CARD);
            _dataManager.AddToQueue((int)RecyclerKeys.SetCard, setCard);
        }

        #endregion

        #region Coroutines

        private IEnumerator WaitToProceed(int key, GameObject model)
        {
            yield return _waitTime;
            _dataManager.AddToQueue(key, model);
        }
        private IEnumerator WaitToProceed(string key, GameObject model)
        {
            yield return _waitTime;
            _dataManager.AddToQueue(key, model);
        }

        private IEnumerator AwaitImage(GameObject model, string cardID)
        {
            yield return new WaitForSeconds(0.75f);

            var imageSetter = model.GetComponentInChildren<IImageSetter>();
            var texture = _dataManager.GetCachedImage(cardID);
            imageSetter.ChangeImage(texture);
        }

        #endregion
    }
}
