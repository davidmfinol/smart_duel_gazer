using System.Collections.Generic;
using Code.Core.DataManager;
using Code.Core.DataManager.GameObjects.Entities;
using Code.Core.General.Extensions;
using Code.Features.SpeedDuel.PrefabManager;
using Code.Features.SpeedDuel.PrefabManager.Prefabs.Playfield.Scripts;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Zenject;

namespace Code.Features.SpeedDuel.EventHandlers
{
    public class PlacementEventHandler : MonoBehaviour
    {
        [SerializeField] private GameObject _playfieldPrefab;
        [SerializeField] private GameObject _placementIndicator;

        private IDataManager _dataManager;
        private PlayfieldEventHandler _playfieldEventHandler;
        private PlayfieldComponentsManager.Factory _playfieldFactory;

        private Camera _mainCamera;
        private ARRaycastManager _arRaycastManager;
        private ARPlaneManager _arPlaneManager;
        private GameObject _prefabManager;
        private GameObject _speedDuelField;
        private Pose _placementPose;
        private bool _objectIsPlaced = false;

        #region Properties

        public GameObject SpeedDuelField  => _speedDuelField;
        public void ExecuteEndOfGame() => ExecuteEndGame();

        #endregion

        #region Constructors

        [Inject]
        public void Construct(
            IDataManager dataManager,
            PlayfieldEventHandler playfieldEventHandler,
            PlayfieldComponentsManager.Factory playfieldFactory)
        {
            _dataManager = dataManager;
            _playfieldEventHandler = playfieldEventHandler;
            _playfieldFactory = playfieldFactory;
        }

        #endregion

        #region LifeCycle

        private void Awake()
        {
            GetObjectReferences();
            _playfieldEventHandler.OnRemovePlayfield += RemovePlayfield;
        }

        private void Update()
        {
            UpdatePlacementIndicatorIfNecessary();
        }

        private void OnDestroy()
        {
            _playfieldEventHandler.OnRemovePlayfield -= RemovePlayfield;
            _dataManager.RemoveGameObject(GameObjectKeys.PlayfieldKey);
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
                _playfieldEventHandler.ActivatePlayfield();
                return;
            }

            #endif

            #endregion

            if (_objectIsPlaced) return;

            var isPlacementAvailable = CheckForAvailablePlanes(out var availablePlanes);

            if (isPlacementAvailable && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                PlaceObject();
                SetPlaymatScale(availablePlanes);
                _playfieldEventHandler.ActivatePlayfield();
            }
        }

        private bool CheckForAvailablePlanes(out List<ARRaycastHit> availablePlanes)
        {
            var hitsList = FindAvailablePlanes();

            var placementPoseIsValid = hitsList.Count > 0;
            if (!placementPoseIsValid)
            {
                _placementIndicator.SetActive(false);
                availablePlanes = hitsList;
                return placementPoseIsValid;
            }

            _placementPose = hitsList[hitsList.Count - 1].pose;
            UpdatePlacementIndicator();

            availablePlanes = hitsList;
            return placementPoseIsValid;
        }

        private List<ARRaycastHit> FindAvailablePlanes()
        {
            var screenCenter = _mainCamera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
            var hitsList = new List<ARRaycastHit>();
            _arRaycastManager.Raycast(screenCenter, hitsList, TrackableType.Planes);

            return hitsList;
        }

        private void UpdatePlacementIndicator()
        {
            var cameraForward = _mainCamera.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            _placementPose.rotation = Quaternion.LookRotation(cameraBearing);

            _placementIndicator.SetActive(true);
            _placementIndicator.transform.SetPositionAndRotation(_placementPose.position, _placementPose.rotation);        
        }

        private void PlaceObject()
        {
            _objectIsPlaced = true;
            _placementIndicator.SetActive(false);

            var playMat = _dataManager.GetGameObject(GameObjectKeys.PlayfieldKey);
            if (playMat == null)
            {
                CreatePlaymat();
                return;
            }

            playMat.transform.SetPositionAndRotation(_placementPose.position, _placementPose.rotation);
            _playfieldEventHandler.ActivatePlayfield();
        }

        private void CreatePlaymat()
        {
            _speedDuelField = _playfieldFactory.Create(_playfieldPrefab).gameObject;
            _speedDuelField.transform.SetPositionAndRotation(_placementPose.position, _placementPose.rotation);

            //Move Prefab Manager to Playfield for proper card scaling
            _prefabManager.transform.SetParent(_speedDuelField.transform);
            _prefabManager.transform.SetPositionAndRotation(_speedDuelField.transform.position,
                _speedDuelField.transform.rotation);

            _playfieldEventHandler.ActivatePlayfield();
        }

        private void SetPlaymatScale(List<ARRaycastHit> hits)
        {
            if (hits == null) return;

            var plane = _arPlaneManager.GetPlane(hits[hits.Count - 1].trackableId);
            var scalePlane = GetCameraOrientation(plane);

            if (scalePlane <= 0) return;

            _speedDuelField.transform.localScale = new Vector3(scalePlane, scalePlane, scalePlane);

            //disable AR Plane Manager to stop tracking new planes
            _arPlaneManager.SetTrackablesActive(false);
            _arPlaneManager.enabled = false;
        }

        private float GetCameraOrientation(ARPlane plane)
        {
            var cameraOrientation = _mainCamera.transform.rotation.y;

            if (cameraOrientation.IsWithinRange(45, 135) ||
                cameraOrientation.IsWithinRange(225, 315) ||
                cameraOrientation.IsWithinRange(-45, -135) ||
                cameraOrientation.IsWithinRange(-225, -315))
            {
                return plane.size.y; ;
            }

            return plane.size.x;
        }

        private void RemovePlayfield()
        {
            _objectIsPlaced = false;
            _placementIndicator.SetActive(true);
            _arPlaneManager.enabled = true;

            _dataManager.SaveGameObject(GameObjectKeys.PlayfieldKey, _speedDuelField);
        }

        private void ExecuteEndGame()
        {
            Destroy(_speedDuelField);
        }
    }
}