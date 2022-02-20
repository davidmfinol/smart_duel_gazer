using Code.Core.DataManager;
using Code.Core.Logger;
using Code.Features.SpeedDuel.PrefabManager;
using Code.Features.SpeedDuel.PrefabManager.Prefabs.Playfield.Scripts;
using UnityEngine;
using Zenject;

namespace Code.Features.SpeedDuel.EventHandlers.Placement.Nreal
{
    public class TiltFivePlacementEventHandler : MonoBehaviour
    {
        private const string Tag = "TiltFivePlacementEventHandler";

        private const float SpeedDuelFieldScaleFactor = 2;

        [SerializeField] private GameObject playfieldPrefab;
        [SerializeField] private GameObject placementIndicator;

        private IDataManager _dataManager;
        private IPlayfieldEventHandler _playfieldEventHandler;
        private PlayfieldComponentsManager.Factory _playfieldFactory;
        private IAppLogger _logger;

        private GameObject _prefabManager;
        
        private GameObject _speedDuelField;

        #region Construct

        [Inject]
        public void Construct(
            IDataManager dataManager,
            IPlayfieldEventHandler playfieldEventHandler,
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

        private void Start()
        {
            PlacePlayfield();
        }

        private void OnDestroy()
        {
            _playfieldEventHandler.OnRemovePlayfield -= RemovePlayfield;
        }

        #endregion

        private void GetObjectReferences()
        {
            _prefabManager = FindObjectOfType<SpeedDuelPrefabManager>().gameObject;
        }

        #region Playfield

        private void PlacePlayfield()
        {
            _logger.Log(Tag, "PlacePlayfield()");

            placementIndicator.SetActive(false);

            var playfield = _dataManager.GetPlayfield();
            if (playfield == null)
            {
                CreatePlayfield();
            }
            else
            {
                playfield.transform.SetPositionAndRotation(placementIndicator.transform.position, placementIndicator.transform.rotation);
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
            _speedDuelField.transform.SetPositionAndRotation(placementIndicator.transform.position, placementIndicator.transform.rotation);

            // Make Prefab Manager a child of Playfield for proper model scaling
            _prefabManager.transform.SetParent(_speedDuelField.transform);
            _prefabManager.transform.SetPositionAndRotation(_speedDuelField.transform.position,
                _speedDuelField.transform.rotation);

            // Scale up Playfield to fill up the GameBoard
            _speedDuelField.transform.localScale = new Vector3(SpeedDuelFieldScaleFactor, SpeedDuelFieldScaleFactor, SpeedDuelFieldScaleFactor);
        }
        
        #endregion

        private void RemovePlayfield()
        {
            _logger.Log(Tag, "RemovePlayfield()");

            placementIndicator.SetActive(true);
        }
        
    }
}