using UnityEngine;

namespace AssemblyCSharp.Assets.Code.Features.SpeedDuel
{
    public class SetSingleMeshCharacter : MonoBehaviour, ISetMeshCharacter
    {
        [SerializeField]
        private ParticleSystem _particles;

        public void GetCharacterMesh(SkinnedMeshRenderer _skinnedMesh)
        {
            var shape = _particles.shape;
            shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
            shape.skinnedMeshRenderer = _skinnedMesh;
        }
    }
}
