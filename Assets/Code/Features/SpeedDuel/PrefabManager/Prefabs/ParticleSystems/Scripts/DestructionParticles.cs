using Code.Core.Models.ModelEventsHandler;
using UnityEngine;
using Zenject;

namespace Code.Features.SpeedDuel.PrefabManager.Prefabs.ParticleSystems.Scripts
{
    public class DestructionParticles : MonoBehaviour
    {
        private ModelEventHandler _eventHandler;
        private ParticleSystem _particleSystem;

        #region Constructor

        [Inject]
        public void Construct(
            ModelEventHandler modelEventHandler)
        {
            _eventHandler = modelEventHandler;
        }

        #endregion

        #region LifeCycle

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        private void OnEnable()
        {
            _eventHandler.OnMonsterRemoval += OnMonsterDestruction;
        }

        #endregion

        private void OnMonsterDestruction(SkinnedMeshRenderer[] renderers)
        {
            SetMeshShape(renderers[0]);
            _particleSystem.Play();
            _eventHandler.OnMonsterRemoval -= OnMonsterDestruction;
        }

        private void SetMeshShape(SkinnedMeshRenderer skinnedMesh)
        {
            var shape = _particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
            shape.skinnedMeshRenderer = skinnedMesh;
        }

        public class Factory : PlaceholderFactory<GameObject, DestructionParticles>
        {
        }
    }
}
