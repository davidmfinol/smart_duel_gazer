using Code.Core.DataManager;
using Code.Core.DataManager.GameObjects.Entities;
using Code.Features.SpeedDuel.PrefabManager.Prefabs.ParticleSystems.Scripts;
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
        private const int AmountToInstantiate = 16;

        [SerializeField] private GameObject _particles;
        [SerializeField] private GameObject _setCard;

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
            InstantiatePrefabs(GameObjectKeys.SetCardKey, AmountToInstantiate);
            InstantiatePrefabs(GameObjectKeys.ParticlesKey, AmountToInstantiate);

            // TODO: pre-instantiate models from deck:
            // DeckLists of duelists are available when a duel starts via a DuelRoom object
        }

        #endregion

        private void InstantiatePrefabs(string key, int amount)
        {
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
            return key switch
            {
                GameObjectKeys.SetCardKey => _setCardFactory.Create(_setCard).transform.gameObject,
                GameObjectKeys.ParticlesKey => _particleFactory.Create(_particles).transform.gameObject,
                _ => null,
            };
        }
    }
}