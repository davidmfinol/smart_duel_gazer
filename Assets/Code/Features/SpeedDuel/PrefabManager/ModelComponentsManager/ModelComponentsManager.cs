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
        private PlayfieldEventHandler _playfieldEventHandler;

        private Animator _animator;
        private SkinnedMeshRenderer[] _renderers;
        private ModelSettings _settings;
        private int _instanceID;
        private bool _areRenderersEnabled;

        #region Constructor

        [Inject]
        public void Construct(
            ModelEventHandler modelEventHandler,
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

            //Gets InstanceID of parent Game Object
            _instanceID = transform.parent.gameObject.GetInstanceID();
            SubscribeToEvents();
        }

        #endregion

        #region Event Subscriptions

        private void SubscribeToEvents()
        {
            _modelEventHandler.OnActivateModel += ActivateModel;
            _modelEventHandler.OnSummon += SummonMonster;
            _modelEventHandler.OnAction += Action;
            _modelEventHandler.OnRemove += RemoveMonster;

            _playfieldEventHandler.OnActivatePlayfield += ActivatePlayfield;
            _playfieldEventHandler.OnPickupPlayfield += PickupPlayfield;            
        }

        #endregion

        #region Events

        private void ActivateModel(int instanceID)
        {
            if (!gameObject.IsIntendedGameObject(_instanceID, instanceID)) return;

            // Scale model based on ModelSettings
            transform.parent.transform.localScale = _settings.ModelScale;
        }

        private void SummonMonster(int instanceID)
        {
            if (!gameObject.IsIntendedGameObject(_instanceID, instanceID)) return;

            _renderers.SetRendererVisibility(true);
            _areRenderersEnabled = true;
            _animator.SetBool(AnimatorParameters.DefenceBool, false);
            _animator.SetTrigger(AnimatorParameters.SummoningTrigger);
        }

        private void Action(ModelEvent eventName, int instanceID, bool state)
        {
            if (!gameObject.IsIntendedGameObject(_instanceID, instanceID)) return;

            switch (eventName)
            {
                case ModelEvent.Attack:
                    Attack();
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
            if (_instanceID != instanceID || gameObject.activeSelf == false) return;

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

        private void ActivatePlayfield(GameObject playfield)
        {
            _renderers.SetRendererVisibility(_areRenderersEnabled);
        }

        private void PickupPlayfield()
        {
            _renderers.SetRendererVisibility(false);
        }

        #endregion

        #region ModelFunctions

        private void Attack()
        {
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
