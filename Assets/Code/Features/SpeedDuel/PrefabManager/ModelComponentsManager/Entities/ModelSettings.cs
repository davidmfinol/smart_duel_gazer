using Code.Core.DataManager.GameObjects.Entities;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Features.SpeedDuel.PrefabManager.ModelComponentsManager.Entities
{
    public class ModelSettings : MonoBehaviour
    {
        private Vector3 _modelScale = new Vector3( 1, 1, 1);

        [SerializeField] private Transform _target;
        [SerializeField] private bool _hasProjectileAttack;
        [SerializeField] private List<Transform> _projectileSpawnPoints;
        [SerializeField] private ProjectileKeys _projectileType;

        [HideInInspector]public Vector3 ModelScale => _modelScale;
        [HideInInspector] public Transform Target => _target;
        [HideInInspector] public bool HasProjectileAttack => _hasProjectileAttack;
        [HideInInspector] public List<Transform> ProjectileSpawnPoints => _projectileSpawnPoints;
        [HideInInspector] public string ProjectileKey => _projectileType.ToString();
    }
}
