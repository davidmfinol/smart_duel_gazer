using System.Collections.Generic;
using Code.Core.DataManager;
using Code.Core.General.Extensions;
using Code.Features.SpeedDuel.PrefabManager;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Zenject;

namespace Code.Features.SpeedDuel.EventHandlers
{
    public class PlacementEventHandler : MonoBehaviour
    {
        private const string PlayfieldKey = "Playfield";

        [SerializeField] private GameObject _playfieldPrefab;
        [SerializeField] private GameObject _placementIndicator;

        private IDataManager _dataManager;
        private PlayfieldEventHandler _playfieldEventHandler;

        private Camera _mainCamera;
        private ARRaycastManager _arRaycastManager;
        private ARPlaneManager _arPlaneManager;
        private GameObject _prefabManager;
        private GameObject _speedDuelField;
        private Pose _placementPose;
        private bool _placementPoseIsValid = false;
        private bool _objectIsPlaced = false;

        #region Properties

        public GameObject SpeedDuelField
        {
            get => _speedDuelField;
        }

        #endregion

        #region Constructors

        [Inject]
        public void Construct(
            IDataManager dataManager,
            PlayfieldEventHandler playfieldEventHandler)
        {
            _dataManager = dataManager;
            _playfieldEventHandler = playfieldEventHandler;
        }

        #endregion

        #region LifeCycle

        private void Awake()
        {
            GetObjectReferences();
            _playfieldEventHandler.OnRemovePlayfield += OnPlaymatDestroyed;
        }

        private void Update()
        {
            UpdatePlacementIndicatorIfNecessary();
        }

        private void OnDestroy()
        {
            _playfieldEventHandler.OnRemovePlayfield -= OnPlaymatDestroyed;
            _dataManager.RemoveGameObject(PlayfieldKey);
        }

        #endregion

        private void GetObjectReferences()
        {
            _mainCamera = Camera.main;
            _arRaycastManager = FindObjectOfType<ARRaycastManager>();
            _arPlaneManager = FindObjectOfType<ARPlaneManager>();
            _prefabManager = FindObjectOfType<SpeedDuelPrefabManager>().gameObject;
        }

        private void UpdatePlacementIndicatorIfNecessary()
        {

            #region Editor

            #if UNITY_EDITOR

            // Use Spacebar to place playfield if in Editor
            if (!_objectIsPlaced && Input.GetKeyDown(KeyCode.Space))
            {
                PlaceObject();
                _playfieldEventHandler.ActivatePlayfield(_speedDuelField);
                return;
            }

            #endif

            #endregion

            if (_objectIsPlaced)
            {
                return;
            }

            UpdatePlacementPose(out var hits);
            UpdatePlacementIndicator();

            if (_placementPoseIsValid && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                PlaceObject();
                SetPlaymatScale(hits);
                _playfieldEventHandler.ActivatePlayfield(_speedDuelField);
            }
        }

        private void UpdatePlacementPose(out List<ARRaycastHit> hits)
        {
            var screenCenter = _mainCamera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
            var hitsList = new List<ARRaycastHit>();
            _arRaycastManager.Raycast(screenCenter, hitsList, TrackableType.Planes);

            _placementPoseIsValid = hitsList.Count > 0;
            if (_placementPoseIsValid)
            {
                _placementPose = hitsList[hitsList.Count - 1].pose;

                var cameraForward = _mainCamera.transform.forward;
                var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
                _placementPose.rotation = Quaternion.LookRotation(cameraBearing);
            }

            hits = hitsList;
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
            _objectIsPlaced = true;
            _placementIndicator.SetActive(false);

            var playMat = _dataManager.GetGameObject(PlayfieldKey);
            if (playMat == null)
            {
                _speedDuelField = Instantiate(_playfieldPrefab, _placementPose.position, _placementPose.rotation);
                _prefabManager.transform.SetParent(_speedDuelField.transform);
                _prefabManager.transform.SetPositionAndRotation(_speedDuelField.transform.position,
                    _speedDuelField.transform.rotation);
                _playfieldEventHandler.ActivatePlayfield(_speedDuelField);
            }
            else
            {
                playMat.transform.SetPositionAndRotation(_placementPose.position, _placementPose.rotation);
                _playfieldEventHandler.ActivatePlayfield(_speedDuelField);
            }
        }

        private void SetPlaymatScale(List<ARRaycastHit> hits)
        {
            if (hits == null)
            {
                return;
            }

            var plane = _arPlaneManager.GetPlane(hits[hits.Count - 1].trackableId);            
            GetCameraOrientation(plane, out var scalePlane);
            if (scalePlane <= 0)
            {
                return;
            }

            _speedDuelField.transform.localScale = new Vector3(scalePlane, scalePlane, scalePlane);

            _arPlaneManager.SetTrackablesActive(false);
            _arPlaneManager.enabled = false;
        }

        private void GetCameraOrientation(ARPlane plane, out float scalePlane)
        {
            var cameraOrientation = _mainCamera.transform.rotation.y;

            if (cameraOrientation.IsWithinRange(45, 135) ||
                cameraOrientation.IsWithinRange(225, 315) ||
                cameraOrientation.IsWithinRange(-45, -135) ||
                cameraOrientation.IsWithinRange(-225, -315))
            {
                scalePlane = plane.size.y;
                return;
            }

            scalePlane = plane.size.x;
        }

        private void OnPlaymatDestroyed()
        {
            _objectIsPlaced = false;
            _placementIndicator.SetActive(true);
            _arPlaneManager.enabled = true;

            _dataManager.SaveGameObject(PlayfieldKey, _speedDuelField);
            _playfieldEventHandler.PickupPlayfield();
        }
    }
}