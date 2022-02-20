using Code.Core.DataManager;
using Code.Core.Logger;
using Code.Features.SpeedDuel.PrefabManager;
using Code.Features.SpeedDuel.PrefabManager.Prefabs.Playfield.Scripts;
// TODO: using Code.Wrappers.WrapperNreal;
using UnityEngine;
using Zenject;

namespace Code.Features.SpeedDuel.EventHandlers.Placement.Nreal
{
    public class TiltFivePlacementEventHandler : MonoBehaviour
    {
        private const string Tag = "TiltFivePlacementEventHandler";

        [SerializeField] private GameObject playfieldPrefab;
        [SerializeField] private GameObject placementIndicator;

        private IDataManager _dataManager;
        private IPlayfieldEventHandler _playfieldEventHandler;
        private PlayfieldComponentsManager.Factory _playfieldFactory;
        // TODO: private INrealSessionManagerWrapper _nrealSessionManager;
        private IAppLogger _logger;

        // TODO: private NrealPlaneManager _planeManager;
        private GameObject _prefabManager;
        
        private GameObject _speedDuelField;
        private Pose _placementPose;
        private bool _objectPlaced;
        
    }
}