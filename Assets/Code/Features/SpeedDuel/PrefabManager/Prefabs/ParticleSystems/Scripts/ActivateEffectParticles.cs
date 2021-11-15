using Code.Core.DataManager;
using Code.Core.DataManager.GameObjects.Entities;
using Code.Core.Logger;
using UnityEngine;
using Zenject;

namespace Code.Features.SpeedDuel.PrefabManager.Prefabs.ParticleSystems.Scripts
{
    public class ActivateEffectParticles : MonoBehaviour
    {
        private const string Tag = "ActivateEffectParticles";

        private IDataManager _dataManager;
        private IAppLogger _logger;

        #region Constructor

        [Inject]
        public void Construct(
            IDataManager dataManager,
            IAppLogger appLogger)
        {
            _dataManager = dataManager;
            _logger = appLogger;
        }

        #endregion

        #region LifeCycle

        private void Awake()
        {
            var particles = GetComponent<ParticleSystem>().main;
            particles.stopAction = ParticleSystemStopAction.Callback;
        }

        private void OnParticleSystemStopped()
        {
            _logger.Log(Tag, "OnParticleSystemStopped()");

            var go = gameObject;
            go.SetActive(false);
            _dataManager.SaveGameObject(GameObjectKeys.ActivateEffectParticlesKey, go);
        }

        #endregion

        public class Factory : PlaceholderFactory<GameObject, ActivateEffectParticles>
        {
        }
    }
}