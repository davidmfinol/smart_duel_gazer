using Code.Core.Config.Providers;
using Code.Core.DataManager;
using Code.Core.General.Extensions;
using Code.Core.Logger;
using Code.Features.SpeedDuel.PrefabManager.ModelComponentsManager.Entities;
using System.Collections;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Zenject;

namespace Code.Features.SpeedDuel.PrefabManager.Prefabs.Projectiles
{
    [RequireComponent(typeof(ObservableTriggerTrigger))]
    public class Projectile : MonoBehaviour
    {
        private const string Tag = "Projectile";
        
        [SerializeField] private float _speed = 10;

        private ITimeProvider _timeProvider;
        private IAppLogger _logger;

        private ObservableTriggerTrigger _collisionTrigger;

        private string _targetName;
        private Vector3 _shootDir;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        #region Constructor

        [Inject]
        public void Construct(
            ITimeProvider timeProvider,
            IAppLogger appLogger)
        {
            _timeProvider = timeProvider;
            _logger = appLogger;
        }

        #endregion

        #region LifeCycle

        private void Awake()
        {
            _collisionTrigger = GetComponent<ObservableTriggerTrigger>();

            _disposables.Add(_collisionTrigger.OnTriggerEnterAsObservable()
                .Subscribe(HandleCollision));
        }

        private void Update()
        {
            transform.position += _speed * _timeProvider.TimeSinceLastFrame * _shootDir;
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        #endregion

        public void SetTarget(string targetName, Vector3 shootDirection)
        {
            _logger.Log(Tag, $"targetName: {targetName}, shootDirection: {shootDirection}");
            
            _targetName = targetName;
            _shootDir = shootDirection;

            StartCoroutine(DeactivateIfMissedCollider());
        }

        private void HandleCollision(Collider collider)
        {
            _logger.Log(Tag, $"HandleCollision(collider: {collider})");
            
            var hitObject = collider.GetComponentInParent<ModelSettings>().transform.parent;

            if(hitObject.name == _targetName)
            {
                gameObject.SetActive(false);
            }
        }

        private IEnumerator DeactivateIfMissedCollider()
        {
            yield return new WaitForSeconds(3);

            _logger.Log(Tag, "DeactivateIfMissedCollider()");
            
            if(gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        public class Factory : PlaceholderFactory<GameObject, Projectile>
        {
        }
    }
}