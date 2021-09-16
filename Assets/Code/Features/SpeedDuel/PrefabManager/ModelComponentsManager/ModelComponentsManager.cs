using Code.Core.Config.Providers;
using Code.Core.General.Extensions;
using Code.Core.Logger;
using Code.Features.SpeedDuel.EventHandlers;
using Code.Features.SpeedDuel.EventHandlers.Entities;
using Code.Features.SpeedDuel.PrefabManager.ModelComponentsManager.Entities;
using Code.UI_Components.Constants;
using UnityEngine;
using Zenject;

namespace Code.Features.SpeedDuel.PrefabManager.ModelComponentsManager
{
    [RequireComponent(typeof(Animator)), RequireComponent(typeof(ModelSettings)), RequireComponent(typeof(ModelMovementHandler))]
    public class ModelComponentsManager : MonoBehaviour
    {
        private const string Tag = "ModelComponentsManager";
        
        private IModelEventHandler _modelEventHandler;
        private PlayfieldEventHandler _playfieldEventHandler;
        private IDelayProvider _delayProvider;
        private IAppLogger _logger;

        private Animator _animator;
        private SkinnedMeshRenderer[] _renderers;
        private ModelSettings _settings;
        private ModelMovementHandler _movementHandler;
        private GameObject _parent;
        private bool _areRenderersEnabled;
        private bool _attackingMonsterIsInDefence;
        private int _waitTimeForEnemyAnimations = 600;

        #region Properties

        public void CallSummonMonster() => SummonMonster(_parent.GetInstanceID());
        public void CallRemoveMonster() => RemoveMonster(_parent.GetInstanceID());
        public void CallTakeDamage() => _animator.SetTrigger(AnimatorParameters.TakeDamageTrigger);

        #endregion

        #region Constructor

        [Inject]
        public void Construct(
            IModelEventHandler modelEventHandler,
            PlayfieldEventHandler playfieldEventHandler,
            IDelayProvider delayProvider,
            IAppLogger appLogger)
        {
            _delayProvider = delayProvider;
            _modelEventHandler = modelEventHandler;
            _playfieldEventHandler = playfieldEventHandler;
            _logger = appLogger;
        }

        #endregion

        #region Lifecycle

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            _settings = GetComponent<ModelSettings>();
            _movementHandler = GetComponent<ModelMovementHandler>();

            _parent = transform.parent.gameObject;
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            _modelEventHandler.OnActivateModel -= ActivateModel;
            _modelEventHandler.OnSummon -= SummonMonster;
            _modelEventHandler.OnAction -= Action;
            _modelEventHandler.OnDirectAttack -= DirectAttack;
            _modelEventHandler.OnRemove -= RemoveMonster;

            _playfieldEventHandler.OnActivatePlayfield -= ActivatePlayfield;
            _playfieldEventHandler.OnRemovePlayfield -= RemovePlayfield;
        }

        #endregion

        #region Event Subscriptions

        private void SubscribeToEvents()
        {
            _modelEventHandler.OnActivateModel += ActivateModel;
            _modelEventHandler.OnSummon += SummonMonster;
            _modelEventHandler.OnAction += Action;
            _modelEventHandler.OnDirectAttack += DirectAttack;
            _modelEventHandler.OnRemove += RemoveMonster;

            _playfieldEventHandler.OnActivatePlayfield += ActivatePlayfield;
            _playfieldEventHandler.OnRemovePlayfield += RemovePlayfield;
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
            _animator.SetBool(AnimatorParameters.DefenceBool, false);
            _animator.SetTrigger(AnimatorParameters.SummoningTrigger);
        }

        private void Action(ModelEvent eventName, int instanceID, bool state)
        {            
            if (!_parent.ShouldModelListenToEvent(instanceID)) return;

            switch (eventName)
            {
                case ModelEvent.Attack:
                    Attack(state);
                    break;
                case ModelEvent.RevealSetMonsterModel:
                    RevealSetMonsterModel();
                    break;
                case ModelEvent.ChangeMonsterVisibility:
                    ChangeMonsterVisibility(state);
                    break;
            }
        }

        private void DirectAttack(int instanceID, Transform playfieldZone)
        {
            if (!_parent.ShouldModelListenToEvent(instanceID)) return;

            _movementHandler.Activate(_parent.transform, playfieldZone.position);
        }

        private void RemoveMonster(int instanceID)
        {
            if (!_parent.ShouldModelListenToEvent(instanceID)) return;

            if (_animator.HasState(0, AnimatorParameters.DeathTrigger))
            {
                _animator.SetTrigger(AnimatorParameters.DeathTrigger);
                return;
            }

            ActivateParticlesAndRemoveModel();
        }

        #endregion

        #region Functions

        #region Playfield Functions

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

        #region ModelFunctions

        //TODO: Add movement functionality
        private async void Attack(bool isAttackingMonster)
        {
            _logger.Log(Tag, $"Model {name} has entered battle. Is attacking monster: {isAttackingMonster}");
            
            if(!isAttackingMonster)
            {
                await _delayProvider.Wait(_waitTimeForEnemyAnimations);

                TakeDamageIfNeeded();
                return;
            }
            
            var isInDefence = _animator.GetBool(AnimatorParameters.DefenceBool);
            if (isInDefence) return;

            _animator.SetTrigger(AnimatorParameters.PlayMonsterAttack1Trigger);
        }

        private void TakeDamageIfNeeded()
        {
            if (_attackingMonsterIsInDefence) return;

            _animator.SetTrigger(AnimatorParameters.TakeDamageTrigger);
        }

        private void RevealSetMonsterModel()
        {
            _animator.SetBool(AnimatorParameters.DefenceBool, true);
        }

        private void ChangeMonsterVisibility(bool state)
        {
            _renderers.SetRendererVisibility(state);
            _areRenderersEnabled = state;
        }

        private void ActivateParticlesAndRemoveModel()
        {
            _modelEventHandler.RaiseMonsterRemovalEvent(_renderers);
            _renderers.SetRendererVisibility(false);
        }

        #endregion

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
