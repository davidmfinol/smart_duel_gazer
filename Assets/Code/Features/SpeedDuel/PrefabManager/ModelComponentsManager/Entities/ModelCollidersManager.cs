using UniRx.Triggers;
using UnityEngine;
using UniRx;
using System;
using Code.Core.Logger;
using Zenject;

namespace Code.Features.SpeedDuel.PrefabManager.ModelComponentsManager.Entities
{
    public class ModelCollidersManager : MonoBehaviour
    {
        private const string Tag = "ModelCollidersManager";
        
        private IAppLogger _logger;
        private ObservableTriggerTrigger _collisionTrigger;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        #region Properties

        private Subject<Collider> _handleTakeDamage = new Subject<Collider>();
        public IObservable<Collider> HandleTakeDamage => _handleTakeDamage;

        #endregion

        #region Constructor

        [Inject]
        public void Construct(
            IAppLogger appLogger)
        {
            _logger = appLogger;
        }

        #endregion

        #region LifeCycle

        private void Awake()
        {
            _collisionTrigger = GetComponentInChildren<ObservableTriggerTrigger>();

            if(_collisionTrigger == null)
            {
                _logger.Warning(Tag, $"No Observable Available, Creating One: {transform.name}");
                HandleNoObservableWarning();
            }

            BindObservables();
        }

        private void OnDestroy()
        {
            _handleTakeDamage?.Dispose();
            _disposables.Dispose();
        }

        #endregion

        private void BindObservables()
        {
            _disposables.Add(_collisionTrigger.OnTriggerEnterAsObservable()
                .Subscribe(HandleCollisionEvent));
        }

        private void HandleCollisionEvent(Collider collider)
        {
            _handleTakeDamage.OnNext(collider);
        }

        private void HandleNoObservableWarning()
        {
            var collider = GetComponentInChildren<Collider>();
            _collisionTrigger = collider.gameObject.AddComponent<ObservableTriggerTrigger>();
        }
    }
}