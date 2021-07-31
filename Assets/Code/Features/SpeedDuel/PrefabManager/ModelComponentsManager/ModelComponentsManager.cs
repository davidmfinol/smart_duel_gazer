using Code.Features.SpeedDuel.EventHandlers;
using Code.Features.SpeedDuel.PrefabManager.ModelComponentsManager.Entities;
using Code.UI_Components.Constants;
using UnityEngine;
using Zenject;

namespace Code.Features.SpeedDuel.PrefabManager.ModelComponentsManager
{
    public interface IModelComponentsManager
    {
        public void SubscribeToEvents();
        public void UnsubscribeToEvents();
    }
    
    [RequireComponent(typeof(Animator)), RequireComponent(typeof(ModelSettings))]
    public class ModelComponentsManager : MonoBehaviour, IModelComponentsManager
    {
        private ModelEventHandler _modelEventHandler;
        private PlayfieldEventHandler _playfieldEventHandler;

        private Animator _animator;
        private SkinnedMeshRenderer[] _renderers;
        private ModelSettings _settings;
        private int _instanceID;
        private bool _areRenderersEnabled;

        #region Constructor

        [Inject]
        public void Construct(ModelEventHandler modelEventHandler,
                              PlayfieldEventHandler playfieldEventHandler)
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

            _instanceID = transform.GetInstanceID();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeToEvents();
        }

        #endregion

        #region Event Subscriptions

        public void SubscribeToEvents()
        {
            _modelEventHandler.OnActivateModel += ActivateModel;
            _modelEventHandler.OnSummonMonster += SummonMonster;
            _modelEventHandler.OnRevealSetMonster += RevealSetMonster;
            _modelEventHandler.OnChangeMonsterVisibility += SetMonsterVisibility;
            _modelEventHandler.OnDestroyMonster += DestroyMonster;
            _modelEventHandler.OnAttack += Attack;

            _playfieldEventHandler.OnActivatePlayfield += ActivatePlayfield;
            _playfieldEventHandler.OnPickupPlayfield += PickupPlayfield;            
        }

        public void UnsubscribeToEvents()
        {
            _modelEventHandler.OnActivateModel -= ActivateModel;
            _modelEventHandler.OnSummonMonster -= SummonMonster;
            _modelEventHandler.OnRevealSetMonster -= RevealSetMonster;
            _modelEventHandler.OnChangeMonsterVisibility -= SetMonsterVisibility;
            _modelEventHandler.OnDestroyMonster -= DestroyMonster;
            _modelEventHandler.OnAttack -= Attack;

            _playfieldEventHandler.OnActivatePlayfield -= ActivatePlayfield;
            _playfieldEventHandler.OnPickupPlayfield -= PickupPlayfield;
        }

        #endregion

        private void ActivateModel(int instanceID)
        {
            if(_instanceID != instanceID)
            {
                return;
            }
            
            ScaleModel();
        }

        private void ScaleModel()
        {
            transform.parent.transform.localScale = _settings.ModelScale;
        }

        private void SummonMonster(int instanceID)
        {
            if (_instanceID != instanceID)
            {
                return;
            }

            _renderers.SetRendererVisibility(true);
            _areRenderersEnabled = true;
            _animator.SetBool(AnimatorParameters.DefenceBool, false);
            _animator.SetTrigger(AnimatorParameters.SummoningTrigger);
        }

        private void Attack(int instanceID)
        {
            if (_instanceID != instanceID)
            {
                return;
            }

            if (_animator.GetBool("IsDefence"))
            {
                return;
            }

            _animator.SetTrigger(AnimatorParameters.PlayMonsterAttack1Trigger);
        }

        private void RevealSetMonster(int instanceID)
        {
            if (_instanceID != instanceID)
            {
                return;
            }

            _animator.SetBool(AnimatorParameters.DefenceBool, true);
        }

        private void SetMonsterVisibility(int instanceID, bool state)
        {
            if (_instanceID != instanceID)
            {
                return;
            }

            _renderers.SetRendererVisibility(state);
            _areRenderersEnabled = state;
        }

        private void DestroyMonster(int instanceID)
        {
            if (_instanceID != instanceID)
            {
                return;
            }

            if (_animator.HasState(0, AnimatorParameters.DeathTrigger))
            {
                _animator.SetTrigger(AnimatorParameters.DeathTrigger);
                return;
            }
            
            ActivateParticlesAndRemoveModel();
        }

        private void ActivateParticlesAndRemoveModel()
        {
            _modelEventHandler.RaiseMonsterRemovalEvent(_renderers);
            _renderers.SetRendererVisibility(false);
            _modelEventHandler.OnDestroyMonster -= DestroyMonster;
        }

        private void ActivatePlayfield(GameObject playfield)
        {
            _renderers.SetRendererVisibility(_areRenderersEnabled);
        }
        
        private void PickupPlayfield()
        {
            _renderers.SetRendererVisibility(false);
        }

        public class Factory : PlaceholderFactory<GameObject, ModelComponentsManager>
        {
        }
    }

    public static class ModelComponentUtilities
    {
        public static void SetRendererVisibility(this SkinnedMeshRenderer[] renderers, bool visibility)
        {
            foreach (SkinnedMeshRenderer item in renderers)
            {
                item.enabled = visibility;
            }
        }
    }
}
