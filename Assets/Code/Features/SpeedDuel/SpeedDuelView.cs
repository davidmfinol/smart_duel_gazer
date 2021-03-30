using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.General.Statics;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;
using AssemblyCSharp.Assets.Code.Core.Screen.Interface;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Zenject;
using AssemblyCSharp.Assets.Code.Core.YGOProDeck.Impl;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.ModelRecycler.Entities;

namespace AssemblyCSharp.Assets.Code.Features.SpeedDuel
{
    public class SpeedDuelView : MonoBehaviour, ISmartDuelEventListener
    {
        private const string SET_CARD = "SetCard";
        private const string PLAYMAT_ZONES = "Playmat/Zones/";
        private const string CLONE = "(Clone)";
        private const string SPEED_DUEL_FIELD_NAME = "SpeedDuelField";
        private const int _keySetCard = (int)RecyclerKeys.SetCard;
        private const int _keyParticles = (int)RecyclerKeys.DestructionParticles;

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
        private SetImageFromAPIFactory _apiFactory;

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
            IScreenService screenService,
            SetImageFromAPIFactory apiFactory)
        {
            _smartDuelServer = smartDuelServer;
            _dataManager = dataManager;
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
            _dataManager.CreateRecycler();
            InstantiateObjectPool(_keyParticles, _particles, 6);
            BuildObjectInFactory(_keySetCard, _dataManager.GetCardModel(SET_CARD), 6);
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

        private void InstantiateObjectPool(int key, GameObject prefab, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                var obj = Instantiate(prefab, _prefabManager.transform);
                _dataManager.AddToQueue(key, obj);
            }
        }

        private void BuildObjectInFactory(int key, GameObject prefab, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                var obj = _apiFactory.Create(prefab).transform.gameObject;
                obj.transform.SetParent(_prefabManager.transform);
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
            
            if(!_dataManager.CheckForExistingModel(SPEED_DUEL_FIELD_NAME))
            {
                SpeedDuelField = Instantiate(_objectToPlace, _placementPose.position, _placementPose.rotation);
                _prefabManager.transform.SetParent(SpeedDuelField.transform);

                return;
            }

            _dataManager.GetExistingModel(SPEED_DUEL_FIELD_NAME).transform
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

            _dataManager.RecycleModel(SPEED_DUEL_FIELD_NAME, SpeedDuelField);
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
            else if (smartDuelEvent is SpellTrapSetEvent spellTrapSetEvent)
            {
                OnSpellTrapSetEventRecieved(spellTrapSetEvent);
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
                var replacementModel = _dataManager.UseFromQueue(_keySetCard, zone.position, zone.rotation);

                StartCoroutine(AwaitImage(replacementModel, summonEvent.CardId));

                InstantiatedModels.Add(summonEvent.ZoneName + SET_CARD, replacementModel);
                Debug.LogWarning($"No Model for CardId: {summonEvent.CardId}");
                return;
            }

            GameObject instantiatedModel;
            if (_dataManager.CheckForExistingModel(cardModel.name + CLONE))
            {
                instantiatedModel = _dataManager.GetExistingModel(cardModel.name + CLONE, SpeedDuelField.transform);
                instantiatedModel.transform.SetPositionAndRotation(zone.position, zone.rotation);
            }
            else
            {
                instantiatedModel = Instantiate(cardModel, zone.position, zone.rotation, SpeedDuelField.transform);
            }

            var animator = instantiatedModel.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.SetTrigger(AnimatorParams.Summoning_Trigger);
            }

            InstantiatedModels.Add(summonEvent.ZoneName, instantiatedModel);

            if (summonEvent.IsSet)
            {
                //TODO: Add logic for Face Up Defence special summon
                var setCardModel = _dataManager.UseFromQueue(_keySetCard, zone.position, zone.rotation);
                StartCoroutine(AwaitImage(setCardModel, cardModel.name));

                _dataManager.GetMeshRenderers(instantiatedModel.name, instantiatedModel).SetRendererVisibility(false);

                InstantiatedModels.Add(summonEvent.ZoneName + SET_CARD, setCardModel);
            }
        }

        private void OnRemovecardEventReceived(RemoveCardEvent removeCardEvent)
        {
            var zone = SpeedDuelField.transform.Find(PLAYMAT_ZONES + removeCardEvent.ZoneName);

            var modelExists = InstantiatedModels.TryGetValue(removeCardEvent.ZoneName, out var model);
            if (modelExists)
            {
                var characterMesh = _dataManager.GetMeshRenderers(model.name, model);
                characterMesh.SetRendererVisibility(false);
                var destructionParticles = _dataManager.UseFromQueue(_keyParticles, zone.position, zone.rotation, characterMesh[0]);

                StartCoroutine(WaitToProceed(_keyParticles, destructionParticles, 10f));
                StartCoroutine(WaitToProceed(model.name, model, 10f));
                InstantiatedModels.Remove(removeCardEvent.ZoneName);
            }

            var modelIsSet = InstantiatedModels.TryGetValue(removeCardEvent.ZoneName + SET_CARD, out var setCardBack);
            if (!modelIsSet)
            {
                return;
            }

            var animator = setCardBack.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger(AnimatorParams.Remove_Spell_Or_Trap_Trigger);
            }

            InstantiatedModels.Remove(removeCardEvent.ZoneName + SET_CARD);
            StartCoroutine(WaitToProceed(_keySetCard, setCardBack, 5));
        }

        private void OnPositionChangeEventRecieved(PositionChangeEvent positionChangeEvent)
        {
            var zone = SpeedDuelField.transform.Find(PLAYMAT_ZONES + positionChangeEvent.ZoneName);

            var modelExists = InstantiatedModels.TryGetValue(positionChangeEvent.ZoneName, out var model);
            var setCardExists = InstantiatedModels.TryGetValue(positionChangeEvent.ZoneName + SET_CARD, out var setCardModel);
            if (!modelExists && !setCardExists)
            {
                return;
            }

            if (!positionChangeEvent.IsSet)
            {
                //Change into Attack Mode
                _dataManager.GetMeshRenderers(model.name, model).SetRendererVisibility(true);
                return;
            }
            
            if (!setCardExists)
            {
                //Change to Face Down Defence position
                if (_dataManager.GetCardModel(SET_CARD) == null)
                {
                    Debug.LogWarning("There are no Set Card Models. Adding a new one");
                    var newModel = _apiFactory.Create(_dataManager.GetCardModel(SET_CARD)).transform.gameObject;
                    newModel.transform.SetParent(zone);

                    _dataManager.AddToQueue(_keySetCard, newModel);
                }

                _dataManager.GetMeshRenderers(model.name, model).SetRendererVisibility(false);

                var setCard = _dataManager.UseFromQueue(_keySetCard, zone.position, zone.rotation);
                InstantiatedModels.Add(positionChangeEvent.ZoneName + SET_CARD, setCard);
                return;
            }
            
            _dataManager.GetMeshRenderers(model.name, model).SetRendererVisibility(true);
            setCardModel.GetComponent<Animator>().SetTrigger(AnimatorParams.Show_Set_Monster_Trigger);
        }

        private void OnSpellTrapSetEventRecieved(SpellTrapSetEvent spellTrapSetEvent)
        {
            var zone = SpeedDuelField.transform.Find(PLAYMAT_ZONES + spellTrapSetEvent.ZoneName);
            if (zone == null)
            {
                return;
            }
            
            var modelName = spellTrapSetEvent.CardId;
            if (modelName == null)
            {
                return;
            }

            var setCardModel = _dataManager.UseFromQueue(_keySetCard, zone.position, zone.rotation);
            
            if (!_dataManager.CheckForCachedImage(modelName))
            {
                StartCoroutine(AwaitImage(setCardModel, modelName));
                if (InstantiatedModels.ContainsKey(spellTrapSetEvent.ZoneName + SET_CARD))
                {
                    //TODO: Add error handler
                    Debug.LogWarning("Recycling Old Resources");
                    InstantiatedModels.TryGetValue(spellTrapSetEvent.ZoneName + SET_CARD, out var model);
                    _dataManager.AddToQueue(_keySetCard, model);
                    InstantiatedModels.Remove(spellTrapSetEvent.ZoneName + SET_CARD);
                }
                InstantiatedModels.Add(spellTrapSetEvent.ZoneName + SET_CARD, setCardModel);
                return;
            }

            setCardModel.GetComponentInChildren<IImageSetter>().ChangeImageFromAPI(modelName);

            //Temporary activation call for testing. Remove for gameplay
            setCardModel.GetComponent<Animator>()
                .SetTrigger(AnimatorParams.Activate_Spell_Or_Trap_Trigger); ;

            if (InstantiatedModels.ContainsKey(spellTrapSetEvent.ZoneName + SET_CARD))
            {
                //TODO: Add error handler
                Debug.LogWarning("Recycling Old Resources");
                InstantiatedModels.TryGetValue(spellTrapSetEvent.ZoneName + SET_CARD, out var model);
                _dataManager.AddToQueue(_keySetCard, model);
                InstantiatedModels.Remove(spellTrapSetEvent.ZoneName + SET_CARD);
            }
            InstantiatedModels.Add(spellTrapSetEvent.ZoneName + SET_CARD, setCardModel); 
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

        private IEnumerator AwaitImage(GameObject model, string cardID)
        {
            yield return _webRequest.GetRequest(cardID);

            model.GetComponentInChildren<IImageSetter>().ChangeImageFromAPI(cardID);
            model.GetComponent<Animator>()
                .SetTrigger(AnimatorParams.Activate_Spell_Or_Trap_Trigger);
        }

        #endregion
    }
}
