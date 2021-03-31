using UnityEngine;
using AssemblyCSharp.Assets.Code.Core.General;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler;
using AssemblyCSharp.Assets.Code.Core.Models.Interface.ModelComponentsManager;
using AssemblyCSharp.Assets.Code.Core.Models.Interface.ModelEventsHandler.Entities;

namespace AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelComponentsManager
{
    public class ModelComponentsManager : MonoBehaviour, IModelComponentsManager
    {
        [SerializeField]
        private ModelEventHandler _eventHandler;

        private Animator _animator;
        private SkinnedMeshRenderer[] _renderers;
        private string _zone;

        #region Lifecycle

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            _zone = null;
            _eventHandler.OnChangeMonsterVisibility -= SetMonsterVisibility;
        }

        #endregion

        private void SubscribeToEvents()
        {
            _eventHandler.OnSummonMonster += SummonMonster;
            _eventHandler.OnChangeMonsterVisibility += SetMonsterVisibility;
            _eventHandler.OnDestroyMonster += DestroyMonster;
        }

        public void SummonMonster(string zone)
        {
            _zone = zone;
            _renderers.SetRendererVisibility(true);
            _animator.SetTrigger(AnimatorIDSetter.Animator_Summoning_Trigger);
            _eventHandler.OnSummonMonster -= SummonMonster;
        }

        public void SetMonsterVisibility(string zone, bool state)
        {
            if (zone == _zone)
            {
                _renderers.SetRendererVisibility(state);
            }
        }

        public void DestroyMonster(string zone, bool state)
        {
            if (zone == _zone)
            {
                if (_animator.HasState(0, AnimatorIDSetter.Death_Trigger))
                {
                    _animator.SetTrigger(AnimatorIDSetter.Death_Trigger);
                    return;
                }
                ActivateParticlesAndRemoveModel();
            }
        }

        public void ActivateParticlesAndRemoveModel()
        {
            _eventHandler.RaiseEvent(EventNames.OnMonsterDestruction, _renderers);
            _renderers.SetRendererVisibility(false);
            _eventHandler.OnDestroyMonster -= DestroyMonster;
        }
    }
}
