using Code.Core.General.Extensions;
using Code.Core.Logger;
using Code.Features.SpeedDuel.EventHandlers;
using Code.Features.SpeedDuel.EventHandlers.Entities;
using Code.Features.SpeedDuel.PrefabManager.ModelComponentsManager.Entities;
using UnityEngine;
using Zenject;
using UniRx;
using Code.Features.SpeedDuel.PrefabManager.Prefabs.Projectiles;
using Code.Core.DataManager;
using System;

namespace Code.Features.SpeedDuel.PrefabManager.ModelComponentsManager
{
    [RequireComponent(typeof(ModelAnimatorManager)), RequireComponent(typeof(ModelSettings)), RequireComponent(typeof(ModelMovementManager)),
     RequireComponent(typeof(ModelCollidersManager))]
    public class ModelComponentsManager : MonoBehaviour
    {
        private const string Tag = "ModelComponentsManager";

        private IModelEventHandler _modelEventHandler;
        private IPlayfieldEventHandler _playfieldEventHandler;
        private IDataManager _dataManager;
        private IAppLogger _logger;

        private ModelAnimatorManager _animatorManager;
        private ModelCollidersManager _colliderManager;
        private ModelMovementManager _movementManager;
        private ModelSettings _settings;
        private SkinnedMeshRenderer[] _renderers;
        private Projectile.Factory _projectileFactory;
        private GameObject _parent;
        private GameObject _currentTarget;
        private bool _areRenderersEnabled;
        private bool _isInBattle = false;        
        private bool _isAttackingMonster;
       
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        #region Properties

        public void CallSummonMonster() => SummonMonster(_parent.GetInstanceID());
        public void CallRemoveMonster() => RemoveMonster(_parent.GetInstanceID());
        public void CallTakeDamage() => _animatorManager.HandleTakeDamage();
        public void CallAttack() => _animatorManager.PlayAttackAnimation();

        #endregion

        #region Constructor

        [Inject]
        public void Construct(
            IModelEventHandler modelEventHandler,
            IPlayfieldEventHandler playfieldEventHandler,
            IDataManager dataManager,
            Projectile.Factory projectileFactory,
            IAppLogger appLogger)
        {
            _modelEventHandler = modelEventHandler;
            _playfieldEventHandler = playfieldEventHandler;
            _dataManager = dataManager;
            _projectileFactory = projectileFactory;
            _logger = appLogger;
        }

        #endregion

        #region Lifecycle

        private void Awake()
        {
            _animatorManager = GetComponent<ModelAnimatorManager>();
            _colliderManager = GetComponent<ModelCollidersManager>();
            _movementManager = GetComponent<ModelMovementManager>();
            _settings = GetComponent<ModelSettings>();
            _renderers = GetComponentsInChildren<SkinnedMeshRenderer>();

            _parent = transform.parent.gameObject;
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            _modelEventHandler.OnActivateModel -= ActivateModel;
            _modelEventHandler.OnSummon -= SummonMonster;
            _modelEventHandler.OnAction -= Action;
            _modelEventHandler.OnRemove -= RemoveMonster;

            _playfieldEventHandler.OnActivatePlayfield -= ActivatePlayfield;
            _playfieldEventHandler.OnRemovePlayfield -= RemovePlayfield;

            _disposables.Dispose();
        }

        #endregion

        #region Event Subscriptions

        private void SubscribeToEvents()
        {
            // Model Event Handler Events
            _modelEventHandler.OnActivateModel += ActivateModel;
            _modelEventHandler.OnSummon += SummonMonster;
            _modelEventHandler.OnAction += Action;
            _modelEventHandler.OnRemove += RemoveMonster;

            // Playfield Event Handler Events
            _playfieldEventHandler.OnActivatePlayfield += ActivatePlayfield;
            _playfieldEventHandler.OnRemovePlayfield += RemovePlayfield;

            // SubManager Streams
            _disposables.Add(_animatorManager.ActivateParticles
                .Subscribe(_ => ActivateParticlesAndRemoveModel()));
            _disposables.Add(_colliderManager.HandleTakeDamage
                .Subscribe(_ => HandleTakeDamage()));
        }

        #endregion

        #region Events

        private void ActivateModel(int instanceID)
        {
            if (!_parent.ShouldModelListenToEvent(instanceID)) return;

            // Scale model based on ModelSettings
            _parent.transform.localScale = _settings.ModelScale;
        }

        private void SummonMonster(int instanceID)
        {
            if (!_parent.ShouldModelListenToEvent(instanceID)) return;

            _renderers.SetRendererVisibility(true);
            _areRenderersEnabled = true;

            _animatorManager.SummonMonster();
        }

        private void Action(ModelEvent eventName, int instanceID, ModelActionEventArgs args)
        {            
            if (!_parent.ShouldModelListenToEvent(instanceID)) return;

            switch (eventName)
            {
                case ModelEvent.AttackDeclaration:
                    AttackDeclaration(args);
                    break;
                case ModelEvent.DamageStep:
                    DamageStep(args);
                    break;
                case ModelEvent.RevealSetMonsterModel:
                    RevealSetMonsterModel();
                    break;
                case ModelEvent.ChangeMonsterVisibility:
                    ChangeMonsterVisibility(args);
                    break;
            }
        }

        private void RemoveMonster(int instanceID)
        {
            if (!_parent.ShouldModelListenToEvent(instanceID)) return;

            _animatorManager.RemoveMonster();
        }

        #endregion

        #region Functions

        #region Playfield Event Functions

        private void ActivatePlayfield()
        {
            if (!gameObject.activeSelf) return;          
            _renderers.SetRendererVisibility(_areRenderersEnabled);
        }

        private void RemovePlayfield()
        {
            if (!gameObject.activeSelf) return;
            _renderers.SetRendererVisibility(false);
        }

        #endregion

        #region Model Event Functions

        private void AttackDeclaration(ModelActionEventArgs args)
        {
            _logger.Log(Tag, $"AttackDeclaration(args: {args})");

            if (!(args is ModelActionAttackEvent eventArgs)) return;

            _isInBattle = true;
            _isAttackingMonster = eventArgs.IsAttackingMonster;

            if (!_isAttackingMonster)
            {
                _animatorManager.HandleDefendingMonster();
                return;
            }

            HandleAttackingMonster(eventArgs);
        }

        private void HandleAttackingMonster(ModelActionAttackEvent eventArgs)
        {
            _logger.Log(Tag, $"HandleAttackingMonster(eventArgs: {eventArgs})");
            
            if (_settings.HasProjectileAttack)
            {
                DamageStep(eventArgs);
                return;
            }

            var targetTransform = eventArgs.PlayfieldTargetTransform;
            _movementManager.Activate(_parent.transform, targetTransform.position);
        }

        private void DamageStep(ModelActionEventArgs args)
        {
            _logger.Log(Tag, $"DamageStep(args: {args})");
            
            if (!(args is ModelActionAttackEvent eventArgs)) return;

            _currentTarget = eventArgs.AttackTargetGameObject;
            _animatorManager.PlayAttackAnimation();
        }

        private void HandleTakeDamage()
        {
            _logger.Log(Tag, "HandleTakeDamage()");

            if (!_isInBattle || _isAttackingMonster) return;

            _isInBattle = false;
            _animatorManager.HandleTakeDamage();
        }

        private void RevealSetMonsterModel()
        {
            _animatorManager.RevealSetMonster();
        }

        private void ChangeMonsterVisibility(ModelActionEventArgs args)
        {
            if (!(args is ModelActionBoolEvent eventArgs)) return;

            _renderers.SetRendererVisibility(eventArgs.State);
            _areRenderersEnabled = eventArgs.State;
        }

        private void ActivateParticlesAndRemoveModel()
        {
            _modelEventHandler.RaiseMonsterRemovalEvent(_renderers);
            _renderers.SetRendererVisibility(false);
        }

        #endregion

        public void ReturnToZone()
        {
            _logger.Log(Tag, "ReturnToZone()");
            
            _movementManager.ReturnToZone();
        }

        public void FireProjectile()
        {
            _logger.Log(Tag, "FireProjectile()");

            if (!_settings.HasProjectileAttack || _settings.ProjectileSpawnPoints == null) return;

            var targetTransform = _currentTarget.GetComponentInChildren<ModelSettings>().Target;            
            foreach (var spawnPoint in _settings.ProjectileSpawnPoints)
            {                
                var projectile = _dataManager.GetGameObject(_settings.Projectile.name);
                if (projectile == null)
                {
                    projectile = _projectileFactory.Create(_settings.Projectile).gameObject;
                }

                projectile.SetActive(true);
                projectile.transform.position = spawnPoint.position;
                projectile.transform.LookAt(targetTransform);

                projectile.GetComponent<Projectile>().SetTarget(_currentTarget.name, projectile.transform.forward);
            }            
        }

        #endregion

        public class Factory : PlaceholderFactory<GameObject, ModelComponentsManager>
        {
        }
    }

    public static class ModelComponentUtilities
    {
        public static void SetRendererVisibility(this SkinnedMeshRenderer[] renderers, bool visibility)
        {
            foreach (var item in renderers)
            {
                item.enabled = visibility;
            }
        }
    }
}
