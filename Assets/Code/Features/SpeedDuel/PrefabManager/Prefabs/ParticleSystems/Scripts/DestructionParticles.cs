using Code.Features.SpeedDuel.EventHandlers;
using UnityEngine;
using Zenject;

namespace Code.Features.SpeedDuel.PrefabManager.Prefabs.ParticleSystems.Scripts
{
    public class DestructionParticles : MonoBehaviour
    {
        private IModelEventHandler _modelEventHandler;
        private ParticleSystem _particleSystem;

        #region Constructor

        [Inject]
        public void Construct(
            IModelEventHandler modelEventHandler)
        {
            _modelEventHandler = modelEventHandler;
        }

        #endregion

        #region LifeCycle

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        private void OnEnable()
        {
            _modelEventHandler.OnMonsterRemoval += OnMonsterDestruction;
        }

        private void OnDestroy()
        {
            _modelEventHandler.OnMonsterRemoval -= OnMonsterDestruction;
        }

        #endregion

        // TODO: renderers can be empty for custom models
        private void OnMonsterDestruction(SkinnedMeshRenderer[] renderers)
        {
            SetMeshShape(renderers[0]);
            _particleSystem.Play();
            _modelEventHandler.OnMonsterRemoval -= OnMonsterDestruction;
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
