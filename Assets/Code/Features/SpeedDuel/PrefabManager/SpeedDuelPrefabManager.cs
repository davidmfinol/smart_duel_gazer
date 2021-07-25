using Zenject;
using UnityEngine;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Features.SpeedDuel.PrefabManager.Prefabs.SetCard.Scripts;
using AssemblyCSharp.Assets.Code.Features.SpeedDuel.PrefabManager.Prefabs.ParticleSystems.Scripts;

namespace AssemblyCSharp.Assets.Code.Features.SpeedDuel.PrefabManager
{
    /// <summary>
    /// Used for pre-instantiating prefabs that can be reused.
    /// e.g. set cards, destruction particles, monster models, ...
    /// </summary>
    public class SpeedDuelPrefabManager : MonoBehaviour
    {
        private const string ParticlesKey = "Particles";
        private const string SetCardKey = "SetCard";

        private const int AmountToInstantiate = 16;

        [SerializeField]
        private GameObject _particles;
        [SerializeField]
        private GameObject _setCard;

        private IDataManager _dataManager;
        private SetCard.Factory _setCardFactory;
        private DestructionParticles.Factory _particleFactory;

        #region Constructor

        [Inject]
        public void Construct(
            IDataManager dataManager,
            SetCard.Factory setCardFactory,
            DestructionParticles.Factory particlesFactory)
        {
            _dataManager = dataManager;
            _setCardFactory = setCardFactory;
            _particleFactory = particlesFactory;
        }

        #endregion

        #region LifeCycle

        private void Awake()
        {
            InstantiatePrefabs(SetCardKey, AmountToInstantiate);
            InstantiatePrefabs(ParticlesKey, AmountToInstantiate);

            // TODO: pre-instantiate models from deck:
            // Recommend sending over deck information (ie. which/how many models) in order to have them ready
            // when the duel starts. This would be for multiplayer, it's not really needed currently.
        }

        #endregion

        private void InstantiatePrefabs(string key, int amount)
        {
            for (var i = 0; i < amount; i++)
            {
                var gameObject = CreateGameObject(key);
                gameObject.transform.SetParent(transform);                
                _dataManager.SaveGameObject(key, gameObject);

                gameObject.SetActive(false);
            }
        }

        private GameObject CreateGameObject(string key)
        {
            return key switch
            {
                SetCardKey => _setCardFactory.Create(_setCard).transform.gameObject,
                ParticlesKey => _particleFactory.Create(_particles).transform.gameObject,
                _ => null,
            };
        }
    }
}
