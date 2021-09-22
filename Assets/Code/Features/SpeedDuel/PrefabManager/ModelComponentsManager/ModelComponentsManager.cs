using Code.Core.General.Extensions;
using Code.Features.SpeedDuel.EventHandlers;
using Code.Features.SpeedDuel.EventHandlers.Entities;
using Code.Features.SpeedDuel.PrefabManager.ModelComponentsManager.Entities;
using Code.UI_Components.Constants;
using UnityEngine;
using Zenject;

namespace Code.Features.SpeedDuel.PrefabManager.ModelComponentsManager
{    
    [RequireComponent(typeof(Animator)), RequireComponent(typeof(ModelSettings))]
    public class ModelComponentsManager : MonoBehaviour
    {
        private ModelEventHandler _modelEventHandler;
        private IPlayfieldEventHandler _playfieldEventHandler;

        private Animator _animator;
        private SkinnedMeshRenderer[] _renderers;
        private ModelSettings _settings;
        private GameObject _parent;
        private bool _areRenderersEnabled;

        #region Properties

        public void CallSummonMonster() => SummonMonster(_parent.GetInstanceID());
        public void CallRemoveMonster() => RemoveMonster(_parent.GetInstanceID());

        #endregion

        #region Constructor

        [Inject]
        public void Construct(
            ModelEventHandler modelEventHandler,
            IPlayfieldEventHandler playfieldEventHandler)
        {
            _modelEventHandler = modelEventHandler;
            _playfieldEventHandler = playfieldEventHandler;
        }

        #endregion

        #region Lifecycle

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            _settings = GetComponent<ModelSettings>();

            _parent = transform.parent.gameObject;
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            _modelEventHandler.OnActivateModel -= ActivateModel;
            _modelEventHandler.OnSummon -= SummonMonster;
            _modelEventHandler.OnAction -= Action;
            _modelEventHandler.OnRemove -= RemoveMonster;

            _playfieldEventHandler.OnActivatePlayfield -= (_ => ActivatePlayfield());
            _playfieldEventHandler.OnRemovePlayfield -= RemovePlayfield;
        }

        #endregion

        #region Event Subscriptions

        private void SubscribeToEvents()
        {
            _modelEventHandler.OnActivateModel += ActivateModel;
            _modelEventHandler.OnSummon += SummonMonster;
            _modelEventHandler.OnAction += Action;
            _modelEventHandler.OnRemove += RemoveMonster;

            _playfieldEventHandler.OnActivatePlayfield += (_ => ActivatePlayfield());
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
        private void Attack(bool isAttackingMonster)
        {
            if(!isAttackingMonster)
            {
                //TODO: Hook Up "Hurt" Animation to models
                _animator.SetTrigger(AnimatorParameters.TakeDamageTrigger);
                return;
            }
            
            var isInDefence = _animator.GetBool(AnimatorParameters.DefenceBool);
            if (isInDefence) return;

            _animator.SetTrigger(AnimatorParameters.PlayMonsterAttack1Trigger);
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
