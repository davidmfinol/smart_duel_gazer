using System.Collections.Generic;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.Screen.Interface;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Zenject;

namespace AssemblyCSharp.Assets.Code.Features.SpeedDuel
{
    [RequireComponent(typeof(ARRaycastManager))]
    public class SpeedDuelView : MonoBehaviour, ISmartDuelEventListener
    {
        private static readonly int SummoningAnimatorId = Animator.StringToHash("SummoningTrigger");

        [SerializeField]
        private GameObject _objectToPlace;
        [SerializeField]
        private GameObject _placementIndicator;

        private ISmartDuelServer _smartDuelServer;
        private IDataManager _dataManager;

        private ARRaycastManager _arRaycastManager;
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
            IScreenService screenService
        )
        {
            _smartDuelServer = smartDuelServer;
            _dataManager = dataManager;

            screenService.UseAutoOrientation();
            ConnectToServer();
        }

        #endregion

        #region Lifecycle

        private void Start()
        {
            StartPlacementIndicator();
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

        private void StartPlacementIndicator()
        {
            _arRaycastManager = FindObjectOfType<ARRaycastManager>();
        }

        private void UpdatePlacementIndicatorIfNecessary()
        {
            if (_objectPlaced)
            {
                return;
            }

            UpdatePlacementPose();
            UpdatePlacementIndicator();

            if (_placementPoseIsValid && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                PlaceObject();
            }
        }

        private void UpdatePlacementPose()
        {
            var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
            var hits = new List<ARRaycastHit>();
            _arRaycastManager.Raycast(screenCenter, hits, TrackableType.Planes);

            _placementPoseIsValid = hits.Count > 0;
            if (_placementPoseIsValid)
            {
                _placementPose = hits[0].pose;

                var cameraForward = Camera.current.transform.forward;
                var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
                _placementPose.rotation = Quaternion.LookRotation(cameraBearing);
            }
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
        }

        private void OnSummonEventReceived(SummonEvent summonEvent)
        {
            var zone = SpeedDuelField.transform.Find(summonEvent.ZoneName);
            if (zone == null)
            {
                return;
            }

            var cardModel = _dataManager.GetCardModel(summonEvent.CardId);
            if (cardModel == null)
            {
                return;
            }

            var instantiatedModel = Instantiate(cardModel, zone.transform.position, zone.transform.rotation);

            var animator = instantiatedModel.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.SetTrigger(SummoningAnimatorId);
            }

            InstantiatedModels.Add(summonEvent.ZoneName, instantiatedModel);
        }

        private void OnRemovecardEventReceived(RemoveCardEvent removeCardEvent)
        {
            var modelExists = InstantiatedModels.TryGetValue(removeCardEvent.ZoneName, out var model);
            if (!modelExists)
            {
                return;
            }

            Destroy(model);
            InstantiatedModels.Remove(removeCardEvent.ZoneName);
        }

        #endregion
    }
}