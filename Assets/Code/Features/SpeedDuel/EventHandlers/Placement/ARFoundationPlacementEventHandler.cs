using System.Collections.Generic;
using Code.Core.DataManager;
using Code.Core.General.Extensions;
using Code.Core.Logger;
using Code.Features.SpeedDuel.PrefabManager;
using Code.Features.SpeedDuel.PrefabManager.Prefabs.Playfield.Scripts;
using UniRx;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Zenject;

namespace Code.Features.SpeedDuel.EventHandlers.Placement
{
    public class ARFoundationPlacementEventHandler : MonoBehaviour
    {
        private const string Tag = "ARFoundationPlacementEventHandler";
        
        [SerializeField] private GameObject playfieldPrefab;
        [SerializeField] private GameObject placementIndicator;
        [SerializeField] private GameObject ghostField;

        private IDataManager _dataManager;
        private IPlayfieldEventHandler _playfieldEventHandler;
        private SpeedDuelViewModel _speedDuelViewModel;
        private PlayfieldComponentsManager.Factory _playfieldFactory;
        private IAppLogger _logger;

        private Camera _mainCamera;
        private ARRaycastManager _arRaycastManager;
        private ARPlaneManager _arPlaneManager;
        private GameObject _prefabManager;
        
        private GameObject _speedDuelField;
        private Pose _placementPose;
        private TrackableId _placementTrackableId;
        private bool _objectPlaced;
        private bool _settingsMenuActive;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        #region Construct

        [Inject]
        public void Construct(
            IDataManager dataManager,
            IPlayfieldEventHandler playfieldEventHandler,
            SpeedDuelViewModel speedDuelViewModel,
            PlayfieldComponentsManager.Factory playfieldFactory,
            IAppLogger logger)
        {
            _dataManager = dataManager;
            _playfieldEventHandler = playfieldEventHandler;
            _speedDuelViewModel = speedDuelViewModel;
            _playfieldFactory = playfieldFactory;
            _logger = logger;
        }

        #endregion

        #region LifeCycle

        private void Awake()
        {
            GetObjectReferences();
            _playfieldEventHandler.OnRemovePlayfield += RemovePlayfield;

            _disposables.Add(_speedDuelViewModel.SettingsMenuVisibility
                .Subscribe(UpdateSettingsMenuState));
        }

        private void Update()
        {
            #if UNITY_EDITOR

            // Use SpaceBar to place playfield if in Editor
            if (!_objectPlaced && Input.GetKeyDown(KeyCode.Space))
            {
                PlacePlayfield();
                return;
            }

            #endif

            UpdatePlacementIndicatorIfNecessary();
            PlacePlayfieldIfNecessary();
        }

        private void OnDestroy()
        {
            _playfieldEventHandler.OnRemovePlayfield -= RemovePlayfield;
            _disposables.Dispose();
        }

        #endregion

        private void GetObjectReferences()
        {
            _logger.Log(Tag, "GetObjectReferences()");
            
            _mainCamera = Camera.main;
            _arRaycastManager = FindObjectOfType<ARRaycastManager>();
            _arPlaneManager = FindObjectOfType<ARPlaneManager>();
            _prefabManager = FindObjectOfType<SpeedDuelPrefabManager>().gameObject;
        }

        private void UpdateSettingsMenuState(bool newState)
        {
            _logger.Log(Tag, $"UpdateSettingsMenuState(newState: {newState})");
            
            _settingsMenuActive = newState;
        }

        #region Placement Indicator

        private void UpdatePlacementIndicatorIfNecessary()
        {
            if (_objectPlaced) return;
            
            _logger.Log(Tag, "UpdatePlacementIndicatorIfNecessary()");

            var validHit = GetValidRaycastHit();
            if (!validHit.HasValue)
            {
                placementIndicator.SetActive(false);
                return;
            }
            
            UpdatePlacementIndicator();
            SetGameObjectScaleToPlaneSize(ghostField);
        }

        private ARRaycastHit? GetValidRaycastHit()
        {
            _logger.Log(Tag, "GetValidRaycastHit()");
            
            var hitResults = GetRaycastHitResults();
            if (hitResults.Count <= 0)
            {
                return null;
            }

            var validHit = hitResults[hitResults.Count - 1];
            _placementPose = validHit.pose;
            _placementTrackableId = validHit.trackableId;

            return validHit;
        }

        private List<ARRaycastHit> GetRaycastHitResults()
        {
            _logger.Log(Tag, "GetRaycastHitResults()");
            
            var screenCenter = _mainCamera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
            var hitResults = new List<ARRaycastHit>();
            _arRaycastManager.Raycast(screenCenter, hitResults, TrackableType.Planes);

            return hitResults;
        }

        private void UpdatePlacementIndicator()
        {
            _logger.Log(Tag, "UpdatePlacementIndicator()");
            
            var cameraForward = _mainCamera.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            _placementPose.rotation = Quaternion.LookRotation(cameraBearing);
            
            placementIndicator.transform.SetPositionAndRotation(_placementPose.position, _placementPose.rotation);
            placementIndicator.SetActive(true);
        }

        #endregion

        #region Playfield

        private void PlacePlayfieldIfNecessary()
        {
            if (_objectPlaced || !placementIndicator.activeSelf || !HasTouchInput() || _settingsMenuActive) return;
            
            _logger.Log(Tag, "PlacePlayfieldIfNecessary()");
            
            PlacePlayfield();
            SetGameObjectScaleToPlaneSize(_speedDuelField);
            StopPlaneTracking();
        }

        private static bool HasTouchInput()
        {
            return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
        }

        private void PlacePlayfield()
        {
            _logger.Log(Tag, "PlacePlayfield()");
            
            _objectPlaced = true;
            placementIndicator.SetActive(false);

            var playfield = _dataManager.GetPlayfield();
            if (playfield == null)
            {
                CreatePlayfield();
            }
            else
            {
                playfield.transform.SetPositionAndRotation(_placementPose.position, _placementPose.rotation);
            }

            _playfieldEventHandler.ActivatePlayfield();
        }

        private void CreatePlayfield()
        {
            _logger.Log(Tag, "CreatePlayfield()");
            
            _speedDuelField = _playfieldFactory.Create(playfieldPrefab).gameObject;
            _dataManager.SavePlayfield(_speedDuelField);

            // Move Playfield to Scene Root rather than Zenject Project Context
            _speedDuelField.transform.SetParent(transform);
            _speedDuelField.transform.SetPositionAndRotation(_placementPose.position, _placementPose.rotation);

            // Make Prefab Manager a child of Playfield for proper model scaling
            _prefabManager.transform.SetParent(_speedDuelField.transform);
            _prefabManager.transform.SetPositionAndRotation(_speedDuelField.transform.position,
                _speedDuelField.transform.rotation);
        }

        #endregion

        #region ARPlaneManager

        private void SetGameObjectScaleToPlaneSize(GameObject obj)
        {
            _logger.Log(Tag, $"SetGameObjectScaleToPlaneSize(GameObject: {obj})");

            var plane = _arPlaneManager.GetPlane(_placementTrackableId);
            var planeSize = GetPlaneSize(plane, obj);
            if (planeSize <= 0) return;

            obj.transform.localScale = new Vector3(planeSize, planeSize, planeSize);
        }

        private float GetPlaneSize(ARPlane plane, GameObject objectToScale)
        {
            _logger.Log(Tag, $"GetPlaneSize(plane: {plane}, objectToScale: {objectToScale})");

            var cameraForwardRotation = _mainCamera.transform.rotation.y > 0 
                ? _mainCamera.transform.rotation.y 
                : _mainCamera.transform.rotation.y * (-1);

            return cameraForwardRotation.IsWithinRange(0.35f, 0.7f)
                ? plane.size.normalized.y
                : plane.size.normalized.x;
        }

        private void StopPlaneTracking()
        {
            _logger.Log(Tag, "StopPlaneTracking()");

            _arPlaneManager.SetTrackablesActive(false);
            _arPlaneManager.enabled = false;
        }

        #endregion

        private void RemovePlayfield()
        {
            _logger.Log(Tag, "RemovePlayfield()");
            
            _objectPlaced = false;
            placementIndicator.SetActive(true);
            _arPlaneManager.enabled = true;
            foreach(ARPlane plane in _arPlaneManager.trackables)
            {
                plane.gameObject.SetActive(false);
            }
        }
    }
}