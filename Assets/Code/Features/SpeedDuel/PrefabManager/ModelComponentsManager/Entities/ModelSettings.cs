using System.Collections.Generic;
using UnityEngine;

namespace Code.Features.SpeedDuel.PrefabManager.ModelComponentsManager.Entities
{
    public class ModelSettings : MonoBehaviour
    {
        [SerializeField] private Vector3 _modelScale;
        [SerializeField] private Transform _target;
        [SerializeField] private bool _hasProjectileAttack;
        [SerializeField] private List<Transform> _projectileSpawnPoints;
        [SerializeField] private GameObject _projectilePrefab;        

        public Vector3 ModelScale => _modelScale;
        public Transform Target => _target;
        public bool HasProjectileAttack => _hasProjectileAttack;
        public List<Transform> ProjectileSpawnPoints => _projectileSpawnPoints;
        public GameObject Projectile => _projectilePrefab;        
    }
}
