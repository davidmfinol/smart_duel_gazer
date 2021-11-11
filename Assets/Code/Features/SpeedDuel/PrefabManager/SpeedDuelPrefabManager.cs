using Code.Features.SpeedDuel.PrefabManager.Prefabs.ParticleSystems.Scripts;
using Code.Core.DataManager;
using Code.Core.DataManager.GameObjects.Entities;
using Code.Core.Logger;
using Code.Features.SpeedDuel.PrefabManager.Prefabs.SetCard.Scripts;
using UnityEngine;
using Zenject;

namespace Code.Features.SpeedDuel.PrefabManager
{
    /// <summary>
    /// Used for pre-instantiating prefabs that can be reused.
    /// e.g. set cards, destruction particles, monster models, ...
    /// </summary>
    public class SpeedDuelPrefabManager : MonoBehaviour
    {
        private const string Tag = "SpeedDuelPrefabManager";
        private const int AmountToInstantiate = 16;

        [SerializeField] private GameObject _destructionParticles;
        [SerializeField] private GameObject _activateEffectParticles;
        [SerializeField] private GameObject _setCard;

        private IDataManager _dataManager;
        private SetCard.Factory _setCardFactory;
        private DestructionParticles.Factory _particleFactory;
        private ActivateEffectParticles.Factory _effectParticlesFactory;
        private IAppLogger _logger;

        #region Constructor

        [Inject]
        public void Construct(
            IDataManager dataManager,
            SetCard.Factory setCardFactory,
            DestructionParticles.Factory particlesFactory,
            ActivateEffectParticles.Factory effectParticlesFactory,
            IAppLogger appLogger)
        {
            _dataManager = dataManager;
            _setCardFactory = setCardFactory;
            _particleFactory = particlesFactory;
            _effectParticlesFactory = effectParticlesFactory;
            _logger = appLogger;
        }

        #endregion

        #region LifeCycle

        private void Awake()
        {
            InstantiatePrefabs(GameObjectKeys.SetCardKey, AmountToInstantiate);
            InstantiatePrefabs(GameObjectKeys.DestructionParticlesKey, AmountToInstantiate);
            InstantiatePrefabs(GameObjectKeys.ActivateEffectParticlesKey, AmountToInstantiate);

            // TODO: pre-instantiate models from deck:
            // DeckLists of duelists are available when a duel starts via a DuelRoom object
        }

        #endregion

        private void InstantiatePrefabs(string key, int amount)
        {
            _logger.Log(Tag, $"InstantiatePrefabs(key: {key}, amount: {amount})");
            
            for (var i = 0; i < amount; i++)
            {
                var go = CreateGameObject(key);
                if (go == null) continue;

                go.transform.SetParent(transform);
                go.SetActive(false);

                _dataManager.SaveGameObject(key, go);
            }
        }

        private GameObject CreateGameObject(string key)
        {
            _logger.Log(Tag, $"CreateGameObject(key: {key})");
            
            return key switch
            {
                GameObjectKeys.SetCardKey => _setCardFactory.Create(_setCard).transform.gameObject,
                GameObjectKeys.DestructionParticlesKey => _particleFactory.Create(_destructionParticles).transform.gameObject,
                GameObjectKeys.ActivateEffectParticlesKey => _effectParticlesFactory.Create(_activateEffectParticles).transform.gameObject,
                _ => null,
            };
        }
    }
}