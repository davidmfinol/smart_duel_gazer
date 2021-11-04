using UnityEngine;

namespace Code.Features.SpeedDuel.PrefabManager.Prefabs.Projectiles
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float _speed = 10;

        private Vector3 _targetPosition;
        private Vector3 _shootDir;

        void Update()
        {
            transform.position += _speed * Time.deltaTime * _shootDir;

            if(transform.position == _targetPosition)
            {
                Destroy(gameObject);
            }
        }

        public void SetTarget(Transform target, Vector3 shootDirection)
        {
            _targetPosition = target.position;
            _shootDir = shootDirection;
        }

        private void OnTriggerEnter(Collider other)
        {
            Destroy(gameObject);
        }
    }
}