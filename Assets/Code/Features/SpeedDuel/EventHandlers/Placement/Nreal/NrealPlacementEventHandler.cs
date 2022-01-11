using Code.Core.DataManager;
using Code.Core.Logger;
using Code.Features.SpeedDuel.PrefabManager;
using Code.Features.SpeedDuel.PrefabManager.Prefabs.Playfield.Scripts;
using Code.Wrappers.WrapperNreal;
using NRKernal;
using UnityEngine;
using Zenject;

namespace Code.Features.SpeedDuel.EventHandlers.Placement.Nreal
{
    public class NrealPlacementEventHandler : MonoBehaviour
    {
        private const string Tag = "NrealPlacementEventHandler";

        [SerializeField] private GameObject playfieldPrefab;
        [SerializeField] private GameObject placementIndicator;

        private IDataManager _dataManager;
        private IPlayfieldEventHandler _playfieldEventHandler;
        private PlayfieldComponentsManager.Factory _playfieldFactory;
        private INrealSessionManagerWrapper _nrealSessionManager;
        private IAppLogger _logger;

        private NrealPlaneManager _planeManager;
        private GameObject _prefabManager;
        
        private GameObject _speedDuelField;
        private Pose _placementPose;
        private bool _objectPlaced;

        #region Construct

        [Inject]
        public void Construct(
            IDataManager dataManager,
            IPlayfieldEventHandler playfieldEventHandler,
            PlayfieldComponentsManager.Factory playfieldFactory,
            INrealSessionManagerWrapper nrealSessionManager,
            IAppLogger logger)
        {
            _dataManager = dataManager;
            _playfieldEventHandler = playfieldEventHandler;
            _playfieldFactory = playfieldFactory;
            _nrealSessionManager = nrealSessionManager;
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
            _planeManager = FindObjectOfType<NrealPlaneManager>();
            _prefabManager = FindObjectOfType<SpeedDuelPrefabManager>().gameObject;
        }

        #region Placement Indicator

        private void UpdatePlacementIndicatorIfNecessary()
        {
            if (_objectPlaced) return;

            _logger.Log(Tag, "UpdatePlacementIndicatorIfNecessary()");

            var validHit = HasValidRaycastHit();
            if (!validHit)
            {
                placementIndicator.SetActive(false);
                return;
            }

            UpdatePlacementIndicator();
        }

        private bool HasValidRaycastHit()
        {
            _logger.Log(Tag, "HasValidRaycastHit()");

            var handControllerAnchor = NRInput.DomainHand == ControllerHandEnum.Left
                ? ControllerAnchorEnum.LeftLaserAnchor
                : ControllerAnchorEnum.RightLaserAnchor;
            var laserAnchor = NRInput.AnchorsHelper.GetAnchor(NRInput.RaycastMode == RaycastModeEnum.Gaze
                ? ControllerAnchorEnum.GazePoseTrackerAnchor
                : handControllerAnchor);

            if (!Physics.Raycast(new Ray(laserAnchor.transform.position, laserAnchor.transform.forward), out var hitResult, 10) ||
                hitResult.collider.gameObject == null ||
                hitResult.collider.gameObject.GetComponent<NRTrackableBehaviour>() == null) return false;

            var behaviour = hitResult.collider.gameObject.GetComponent<NRTrackableBehaviour>();
            if (behaviour.Trackable.GetTrackableType() != TrackableType.TRACKABLE_PLANE) return false;

            _placementPose = new Pose(hitResult.point, Quaternion.identity);

            return true;
        }

        private void UpdatePlacementIndicator()
        {
            _logger.Log(Tag, "UpdatePlacementIndicator()");

            /*var cameraForward = _mainCamera.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            _placementPose.rotation = Quaternion.LookRotation(cameraBearing);*/

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
            /*SetPlayfieldScale();*/
            StopPlaneTracking();
        }

        private static bool HasTouchInput()
        {
            return NRInput.GetButtonDown(ControllerButton.TRIGGER);
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
        
        /*private void SetPlayfieldScale()
        {
            _logger.Log(Tag, "SetPlayfieldScale()");
            
            var plane = _arPlaneManager.GetPlane(_placementTrackableId);
            var planeSize = GetPlaneSize(plane);
            if (planeSize <= 0) return;

            _speedDuelField.transform.localScale = new Vector3(planeSize, planeSize, planeSize);
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
        }*/

        private void StopPlaneTracking()
        {
            _logger.Log(Tag, "StopPlaneTracking()");
            
            _nrealSessionManager.DisablePlaneDetection();
            
            _planeManager.SetPlaneObjectsActive(false);
        }

        #endregion

        private void RemovePlayfield()
        {
            _logger.Log(Tag, "RemovePlayfield()");

            _objectPlaced = false;
            placementIndicator.SetActive(true);
            
            _planeManager.SetPlaneObjectsActive(true);

            _nrealSessionManager.EnablePlaneDetection();
        }
    }
}