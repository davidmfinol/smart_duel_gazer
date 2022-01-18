using Code.Features.SpeedDuel.PrefabManager.Prefabs.ParticleSystems.Scripts;
using Code.Core.DataManager;
using Code.Core.DataManager.GameObjects.Entities;
using Code.Core.Logger;
using Code.Features.SpeedDuel.PrefabManager.Prefabs.SetCard.Scripts;
using UnityEngine;
using Zenject;
using System.Collections.Generic;
using Code.Core.General.Helpers;
using Code.Features.SpeedDuel.PrefabManager.Prefabs.Projectiles;

namespace Code.Features.SpeedDuel.PrefabManager
{
    /// <summary>
    /// Used for pre-instantiating prefabs that can be reused.
    /// e.g. set cards, destruction particles, monster models, ...
    /// </summary>
    public class SpeedDuelPrefabManager : MonoBehaviour
    {
        private const string Tag = "SpeedDuelPrefabManager";
        private const int AmountToInstantiate = 20;

        [SerializeField] private GameObject destructionParticles;
        [SerializeField] private GameObject activateEffectParticles;
        [SerializeField] private GameObject setCard;
        [SerializeField] private GameObject bulletProjectile;
        [SerializeField] private GameObject fireProjectile;
        [SerializeField] private GameObject magicalProjectile;

        private IDataManager _dataManager;
        private SetCard.Factory _setCardFactory;
        private DestructionParticles.Factory _particleFactory;
        private ActivateEffectParticles.Factory _effectParticlesFactory;
        private Projectile.Factory _projectileFactory;
        private IAppLogger _logger;

        #region Constructor

        [Inject]
        public void Construct(
            IDataManager dataManager,
            SetCard.Factory setCardFactory,
            DestructionParticles.Factory particlesFactory,
            ActivateEffectParticles.Factory effectParticlesFactory,
            Projectile.Factory projectileFactory,
            IAppLogger appLogger)
        {
            _dataManager = dataManager;
            _setCardFactory = setCardFactory;
            _particleFactory = particlesFactory;
            _effectParticlesFactory = effectParticlesFactory;
            _projectileFactory = projectileFactory;
            _logger = appLogger;
        }

        #endregion

        #region LifeCycle

        private void Awake()
        {
            var gameObjectKeys = EnumHelper.GetEnumValues<GameObjectKey>();
            foreach (var key in gameObjectKeys)
            {
                InstantiatePrefabs(key, AmountToInstantiate);
            }

            // TODO: pre-instantiate models from deck:
            // DeckLists of duelists are available when a duel starts via a DuelRoom object
        }

        #endregion

        private void InstantiatePrefabs(GameObjectKey key, int amount)
        {
            _logger.Log(Tag, $"InstantiatePrefabs(key: {key}, amount: {amount})");
            
            for (var i = 0; i < amount; i++)
            {
                var go = CreateGameObject(key);
                if (go == null) continue;

                go.transform.SetParent(transform);
                go.SetActive(false);

                _dataManager.SaveGameObject(key.GetStringValue(), go);
            }
        }

        private GameObject CreateGameObject(GameObjectKey key)
        {
            _logger.Log(Tag, $"CreateGameObject(key: {key})");
            
            return key switch
            {
                GameObjectKey.SetCard => _setCardFactory.Create(setCard).transform.gameObject,
                GameObjectKey.DestructionParticles => _particleFactory.Create(destructionParticles).transform.gameObject,
                GameObjectKey.ActivateEffectParticles => _effectParticlesFactory.Create(activateEffectParticles).transform.gameObject,
                GameObjectKey.BulletProjectile => _projectileFactory.Create(bulletProjectile).transform.gameObject,
                GameObjectKey.FireProjectile => _projectileFactory.Create(fireProjectile).transform.gameObject,
                GameObjectKey.MagicalProjectile => _projectileFactory.Create(magicalProjectile).transform.gameObject,
                _ => null,
            };
        }
    }
}