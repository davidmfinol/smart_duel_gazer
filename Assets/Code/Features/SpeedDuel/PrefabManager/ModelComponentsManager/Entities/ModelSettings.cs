using UnityEngine;

namespace Code.Features.SpeedDuel.PrefabManager.ModelComponentsManager.Entities
{
    public class ModelSettings : MonoBehaviour
    {
        [SerializeField]
        private Vector3 _modelScale;
        public Vector3 ModelScale { get => _modelScale; }
    }
}
