using Code.UI_Components.Constants;
using UnityEngine;
using UniRx;
using System;
using Code.Core.Config.Providers;
using Code.Core.Logger;
using Zenject;
using Animations.Behaviours;

namespace Code.Features.SpeedDuel.PrefabManager.ModelComponentsManager.Entities
{
    [RequireComponent(typeof(Animator))]
    public class ModelAnimatorManager : MonoBehaviour
    {
        private const string Tag = "ModelAnimatorManager";
        
        private IDelayProvider _delayProvider;
        private IAppLogger _logger;

        private Animator _animator;
        private ModelComponentsManager _modelComponentsManager;
        private AttackAnimationObservableTrigger _attackAnimationObservableTrigger;

        private bool _isInDefence;
        private const int _waitForHurtTrigger = 200;

        private CompositeDisposable _disposables = new CompositeDisposable();

        #region Properties

        private readonly Subject<bool> _activateParticles = new Subject<bool>();
        public IObservable<bool> ActivateParticles => _activateParticles;

        #endregion

        #region Constructor

        [Inject]
        public void Construct(
            IDelayProvider delayProvider,
            IAppLogger appLogger)
        {
            _delayProvider = delayProvider;
            _logger = appLogger;
        }

        #endregion

        #region LifeCycle

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _modelComponentsManager = GetComponent<ModelComponentsManager>();

            _attackAnimationObservableTrigger = _animator.GetBehaviour<AttackAnimationObservableTrigger>();

            BindObservables();
        }

        private void OnDestroy()
        {
            _activateParticles?.Dispose();
            _disposables.Dispose();
        }

        #endregion

        private void BindObservables()
        {
            _disposables.Add(_attackAnimationObservableTrigger.OnStateExitAsObservable()
                .Subscribe(_ => _modelComponentsManager.ReturnToZone()));
        }
        
        public void SummonMonster()
        {
            _logger.Log(Tag, "SummonMonster()");
            
            _animator.SetBool(AnimatorParameters.DefenceBool, false);
            _animator.SetTrigger(AnimatorParameters.SummoningTrigger);
        }

        public void RemoveMonster()
        {
            _logger.Log(Tag, "RemoveMonster");
            
            // Death state is a custom animation that not all models have (ie. Buster Blader)
            if (_animator.HasState(0, AnimatorParameters.DeathTrigger))
            {
                _animator.SetTrigger(AnimatorParameters.DeathTrigger);
                return;
            }

            _activateParticles.OnNext(true);
        }

        public async void HandleTakeDamage()
        {
            _logger.Log(Tag, "HandleTakeDamage()");
            
            _animator.SetTrigger(AnimatorParameters.TakeDamageTrigger);

            if (_isInDefence) return;

            // If Model was in Attack Mode before battle, return there
            await _delayProvider.Wait(_waitForHurtTrigger);
            _animator.SetBool(AnimatorParameters.DefenceBool, false);
        }

        public void HandleDefendingMonster()
        {
            _logger.Log(Tag, "HandleDefendingMonster()");
            
            _isInDefence = _animator.GetBool(AnimatorParameters.DefenceBool);
            _animator.SetBool(AnimatorParameters.DefenceBool, true);
        }

        public void RevealSetMonster()
        {
            _logger.Log(Tag, "RevealSetMonster()");
            
            _animator.SetBool(AnimatorParameters.DefenceBool, true);
        }

        public void PlayAttackAnimation()
        {
            _logger.Log(Tag, "PlayAttackAnimation()");
            
            _animator.SetTrigger(AnimatorParameters.PlayMonsterAttack1Trigger);
        }

        // Fired from an Animation Event contained within the 'Attack With Trigger' Animation
        public void HandleAttackAnimationProjectileEvent()
        {
            _logger.Log(Tag, "HandleAttackAnimationEvent()");

            _modelComponentsManager.FireProjectile();
        }
    }
}