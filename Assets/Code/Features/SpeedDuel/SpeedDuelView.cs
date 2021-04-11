using Zenject;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp.Assets.Code.Core.Screen.Interface;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler;
using AssemblyCSharp.Assets.Code.Core.Models.Interface.ModelEventsHandler.Entities;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelComponentsManager;
using AssemblyCSharp.Assets.Code.Core.YGOProDeck.Impl;
using AssemblyCSharp.Assets.Code.Core.General.Statics;

namespace AssemblyCSharp.Assets.Code.Features.SpeedDuel
{
    public class SpeedDuelView : MonoBehaviour, ISmartDuelEventListener
    {
        private static readonly string SET_CARD = "SetCard";
        private static readonly string PLAYMAT_ZONES = "Playmat/Zones/";

        private const string _keyPlayfield = "Playfield";
        private const string _keyParticles = "Particles";
        private const string _keySetCard = "SetCards";

        [SerializeField]
        private GameObject _objectToPlace;
        [SerializeField]
        private GameObject _placementIndicator;
        [SerializeField]
        private GameObject _particles;
        [SerializeField]
        private GameObject _prefabManager;
        

        private ISmartDuelServer _smartDuelServer;
        private IDataManager _dataManager;
        //Change to SetCard Factory to keep names smaller
        private SetImageFromAPIFactory _apiFactory;
        private ModelEventHandler _eventHandler;
        private ModelComponentsManager.Factory _modelFactory;
        private DestructionParticles.Factory _particleFactory;

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
            ModelComponentsManager.Factory modelFactory,
            DestructionParticles.Factory particleFactory,
            SetImageFromAPIFactory apiFactory)
        {
            _smartDuelServer = smartDuelServer;
            _dataManager = dataManager;
            _eventHandler = modelEventHandler;
            _modelFactory = modelFactory;
            _particleFactory = particleFactory;
            _apiFactory = apiFactory;

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
            InstantiateParticles(6);
            InstantiateSetCards(6);
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

        private void InstantiateSetCards(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                var obj = _apiFactory.Create(_dataManager.GetCardModel(SET_CARD)).transform.gameObject;
                obj.transform.SetParent(_prefabManager.transform);
                _dataManager.AddToQueue(_keySetCard, obj);
            }
        }
        private void InstantiateParticles(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                var obj = _particleFactory.Create(_particles).transform.gameObject;
                obj.transform.SetParent(_prefabManager.transform);
                _dataManager.AddToQueue(_keyParticles, obj);
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
                return;
            }
            _placementIndicator.SetActive(false);
        }

        private void PlaceObject()
        {
            _objectPlaced = true;
            _placementIndicator.SetActive(false);
            
            if(!_dataManager.DoesPlayfieldExist())
            {
                SpeedDuelField = Instantiate(_objectToPlace, _placementPose.position, _placementPose.rotation);
                _prefabManager.transform.SetParent(SpeedDuelField.transform);

                return;
            }

            _dataManager.GetFromQueue(_keyPlayfield, _placementPose.position, _placementPose.rotation, null).transform
                .SetPositionAndRotation(_placementPose.position, _placementPose.rotation);
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

        //Check to ensure playmat is still scaling
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

            _dataManager.AddToQueue(_keyPlayfield, SpeedDuelField);
        }

        #endregion

        #region Smart duel events

        private void ConnectToServer()
        {
            _smartDuelServer.Connect(this);
        }

        public void onSmartDuelEventReceived(SmartDuelEvent smartDuelEvent)
        {
            if (smartDuelEvent is PlayCardEvent playCardEvent)
            {
                OnPlayCardEventReceived(playCardEvent);
            }
            else if (smartDuelEvent is RemoveCardEvent removeCardEvent)
            {
                OnRemovecardEventReceived(removeCardEvent);
            }
        }

        //Add logic for spell/trap events
        //Add API stuff
        private void OnPlayCardEventReceived(PlayCardEvent playCardEvent)
        {
            var zone = SpeedDuelField.transform.Find(PLAYMAT_ZONES + playCardEvent.ZoneName);
            if (zone == null)
            {
                return;
            }
            
            var cardModel = _dataManager.GetCardModel(playCardEvent.CardId);
            if (cardModel == null)
            {
                //HandleError rather than return by setting card image from API
                return;
            }

            //Create function for SummonMonster()
            if (!InstantiatedModels.ContainsKey(zone.name))
            {
                GameObject instantiatedModel;
                if (_dataManager.DoesModelExist(cardModel.name))
                {
                    instantiatedModel = _dataManager.GetFromQueue(
                        cardModel.name, zone.position, zone.rotation, SpeedDuelField.transform);
                }
                else
                {
                    instantiatedModel = _modelFactory.Create(cardModel).gameObject.transform.parent.gameObject;
                    instantiatedModel.transform.SetParent(SpeedDuelField.transform);
                    instantiatedModel.transform.SetPositionAndRotation(zone.position, zone.rotation);
                }
                StartCoroutine(AwaitImage(playCardEvent.ZoneName, playCardEvent.CardId));

                _eventHandler.RaiseEvent(EventNames.SummonMonster, zone.name);
                InstantiatedModels.Add(zone.name, instantiatedModel);
            }

            if (playCardEvent.CardPosition == "faceDownDefence")
            {
                Debug.Log(playCardEvent.CardPosition, this);

                if (_dataManager.GetCardModel(SET_CARD) == null)
                {
                    Debug.LogWarning("No Model for Set Card", this);
                    return;
                }

                _eventHandler.RaiseEvent(EventNames.ChangeMonsterVisibility, playCardEvent.ZoneName, false);

                var setCardBack = _dataManager.GetFromQueue(_keySetCard, zone.position, zone.rotation, SpeedDuelField.transform);
                StartCoroutine(AwaitImage(zone.name, cardModel.name));
                InstantiatedModels.Add(playCardEvent.ZoneName + SET_CARD, setCardBack);
            }
            else if (playCardEvent.CardPosition == "faceUpDefence")
            {
                //Add animations & events for set cards with API update
                Debug.Log(playCardEvent.CardPosition, this);

                var hasSetCard = InstantiatedModels.TryGetValue(zone.name + SET_CARD, out var setCardModel);
                if(!hasSetCard)
                {
                    StartCoroutine(AwaitImage(zone.name, cardModel.name));
                    _dataManager.GetFromQueue(_keySetCard, zone.position, zone.rotation, _prefabManager.transform);
                    InstantiatedModels.Add(playCardEvent.ZoneName + SET_CARD, setCardModel);
                }

                _eventHandler.RaiseEvent(EventNames.ChangeMonsterVisibility, playCardEvent.ZoneName, true);

                //Handle error for when image isn't available yet
                _eventHandler.RaiseEvent(EventNames.RevealSetMonster, zone.name);                
            }
            //Add spell/trap faceUp, faceDown logic with API update
        }

        //Go over this again to ensure proper function
        private void OnRemovecardEventReceived(RemoveCardEvent removeCardEvent)
        {
            var modelExists = InstantiatedModels.TryGetValue(removeCardEvent.ZoneName, out var model);
            if (!modelExists)
            {
                var modelSet = InstantiatedModels.TryGetValue(removeCardEvent.ZoneName + SET_CARD, out var setSpellTrap);
                if (!modelSet)
                {
                    return;
                }

                _eventHandler.RaiseEvent(EventNames.SpellTrapRemove, removeCardEvent.ZoneName);
                InstantiatedModels.Remove(removeCardEvent.ZoneName + SET_CARD);
                StartCoroutine(WaitToProceed(_keySetCard, setSpellTrap));
                return;
            }

            var destructionParticles = _dataManager.GetFromQueue(
                _keyParticles, model.transform.position, model.transform.rotation, SpeedDuelField.transform);
            
            _eventHandler.RaiseEvent(EventNames.DestroyMonster, removeCardEvent.ZoneName);

            StartCoroutine(WaitToProceed(_keyParticles, destructionParticles));
            StartCoroutine(WaitToProceed(model.name, model));

            InstantiatedModels.Remove(removeCardEvent.ZoneName);

            var modelIsSet = InstantiatedModels.TryGetValue(removeCardEvent.ZoneName + SET_CARD, out var setCard);
            if (!modelIsSet)
            {
                return;
            }

            _eventHandler.RaiseEvent(EventNames.DestroySetMonster, removeCardEvent.ZoneName);
            InstantiatedModels.Remove(removeCardEvent.ZoneName + SET_CARD);
            StartCoroutine(WaitToProceed(_keySetCard, setCard));

            _dataManager.AddToQueue(_keySetCard, setCard);
        }

        #endregion

        #region Coroutines

        private IEnumerator WaitToProceed(string key, GameObject model)
        {
            yield return _waitTime;
            _dataManager.AddToQueue(key.Split('(')[0], model);
        }

        private IEnumerator AwaitImage(string zone, string cardID)
        {
            yield return _webRequest.GetRequest(cardID);

            _eventHandler.RaiseEvent(EventNames.SummonSpellTrap, zone, cardID);

            //Temporary activation call for testing. Remove for gameplay
            _eventHandler.RaiseEvent(EventNames.SpellTrapActivate, zone);
        }

        #endregion
    }
}
