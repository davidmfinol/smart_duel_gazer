using UniRx.Triggers;
using UnityEngine;
using UniRx;
using System;

namespace Code.Features.SpeedDuel.PrefabManager.ModelComponentsManager.Entities
{
    public class ModelCollidersManager : MonoBehaviour
    {
        private ObservableTriggerTrigger _collisionTrigger;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        #region Properties

        private Subject<Collider> _handleTakeDamage = new Subject<Collider>();
        public IObservable<Collider> HandleTakeDamage => _handleTakeDamage;

        #endregion

        #region LifeCycle

        private void Awake()
        {
            _collisionTrigger = GetComponentInChildren<ObservableTriggerTrigger>();

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
    }
}