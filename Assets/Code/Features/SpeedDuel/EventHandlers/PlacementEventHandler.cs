using System.Collections.Generic;
using Code.Core.DataManager;
using Code.Core.DataManager.GameObjects.Entities;
using Code.Core.General.Extensions;
using Code.Core.Logger;
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
        private const string Tag = "PlacementEventHandler";
        
        [SerializeField] private GameObject playfieldPrefab;
        [SerializeField] private GameObject placementIndicator;

        private IDataManager _dataManager;
        private PlayfieldEventHandler _playfieldEventHandler;
        private PlayfieldComponentsManager.Factory _playfieldFactory;
        private IAppLogger _logger;

        private Camera _mainCamera;
        private ARRaycastManager _arRaycastManager;
        private ARPlaneManager _arPlaneManager;
        private GameObject _prefabManager;

        private Pose _placementPose;
        private TrackableId _placementTrackableId;
        private bool _objectPlaced;

        #region Properties

        public GameObject SpeedDuelField { get; private set; }

        #endregion

        #region Construct

        [Inject]
        public void Construct(
            IDataManager dataManager,
            PlayfieldEventHandler playfieldEventHandler,
            PlayfieldComponentsManager.Factory playfieldFactory,
            IAppLogger logger)
        {
            _dataManager = dataManager;
            _playfieldEventHandler = playfieldEventHandler;
            _playfieldFactory = playfieldFactory;
            _logger = logger;
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
        }

        #endregion

        private void GetObjectReferences()
        {
            _mainCamera = Camera.main;
            _arRaycastManager = FindObjectOfType<ARRaycastManager>();
            _arPlaneManager = FindObjectOfType<ARPlaneManager>();
            _prefabManager = FindObjectOfType<SpeedDuelPrefabManager>().gameObject;
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
            if (_objectPlaced || !placementIndicator.activeSelf || !HasTouchInput()) return;
            
            _logger.Log(Tag, "PlacePlayfieldIfNecessary()");
            
            PlacePlayfield();
            SetPlayfieldScale();
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

            var playfield = _dataManager.GetGameObject(GameObjectKeys.PlayfieldKey);
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
            
            SpeedDuelField = _playfieldFactory.Create(playfieldPrefab).gameObject;

            // Move Playfield to Scene Root rather than Zenject Project Context
            SpeedDuelField.transform.SetParent(transform);
            SpeedDuelField.transform.SetPositionAndRotation(_placementPose.position, _placementPose.rotation);

            // Make Prefab Manager a child of Playfield for proper model scaling
            _prefabManager.transform.SetParent(SpeedDuelField.transform);
            _prefabManager.transform.SetPositionAndRotation(SpeedDuelField.transform.position,
                SpeedDuelField.transform.rotation);
        }

        private void SetPlayfieldScale()
        {
            _logger.Log(Tag, "SetPlayfieldScale()");
            
            var plane = _arPlaneManager.GetPlane(_placementTrackableId);
            var planeSize = GetPlaneSize(plane);
            if (planeSize <= 0) return;

            SpeedDuelField.transform.localScale = new Vector3(planeSize, planeSize, planeSize);
        }

        private float GetPlaneSize(ARPlane plane)
        {
            _logger.Log(Tag, $"GetPlaneSize(plane: {plane})");
            
            var cameraOrientation = _mainCamera.transform.rotation.y;

            if (cameraOrientation.IsWithinRange(45, 135) ||
                cameraOrientation.IsWithinRange(225, 315) ||
                cameraOrientation.IsWithinRange(-45, -135) ||
                cameraOrientation.IsWithinRange(-225, -315))
            {
                return plane.size.y;
            }

            return plane.size.x;
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
            _objectPlaced = false;
            placementIndicator.SetActive(true);
            _arPlaneManager.enabled = true;

            _dataManager.SaveGameObject(GameObjectKeys.PlayfieldKey, SpeedDuelField);
        }
    }
}