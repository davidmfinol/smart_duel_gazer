using UnityEngine;
using AssemblyCSharp.Assets.Code.Core.Models.Interface;
using AssemblyCSharp.Assets.Code.Core.Models.Impl;

namespace AssemblyCSharp.Assets.Code.Features.SpeedDuel
{
    public class DestructionParticles : MonoBehaviour, ISetToMeshShape
    {
        [SerializeField]
        private ModelEventHandler _eventHandler;
        private ParticleSystem _particles;

        #region LifeCycle

        private void Awake()
        {
            _particles = GetComponent<ParticleSystem>();
        }

        private void OnEnable()
        {
            _eventHandler.OnMonsterDestruction += OnMonsterDestruction;
        }

        #endregion

        private void OnMonsterDestruction(SkinnedMeshRenderer[] renderers)
        {
            GetMeshShape(renderers[0]);
            _particles.Play();
            _eventHandler.OnMonsterDestruction -= OnMonsterDestruction;
        }

        public void GetMeshShape(SkinnedMeshRenderer _skinnedMesh)
        {
            var shape = _particles.shape;
            shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
            shape.skinnedMeshRenderer = _skinnedMesh;
        }
    }
}
