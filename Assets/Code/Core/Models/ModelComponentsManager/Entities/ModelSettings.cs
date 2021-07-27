using UnityEngine;

namespace Code.Core.Models.ModelComponentsManager.Entities
{
    public class ModelSettings : MonoBehaviour
    {
        [SerializeField]
        private Vector3 _modelScale;
        public Vector3 ModelScale => _modelScale;
    }
}
