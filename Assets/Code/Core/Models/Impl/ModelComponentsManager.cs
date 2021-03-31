using UnityEngine;
using AssemblyCSharp.Assets.Code.Core.General;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;
using AssemblyCSharp.Assets.Code.Core.Models.Interface;
using AssemblyCSharp.Assets.Code.Core.Models.Interface.Entities;
using AssemblyCSharp.Assets.Code.Core.General.Statics;

namespace AssemblyCSharp.Assets.Code.Core.Models.Impl
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

            SubscribeToEvents();
        }

        private void OnEnable()
        {
            _eventHandler.OnSummonMonster += SummonMonster;
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
            _animator.SetTrigger(AnimatorParams.Summoning_Trigger);
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
                if (_animator.HasState(0, AnimatorParams.Death_Trigger))
                {
                    _animator.SetTrigger(AnimatorParams.Death_Trigger);
                    return;
                }
                BlowUpMonster();
            }
        }

        public void BlowUpMonster()
        {
            _eventHandler.RaiseEvent(EventNames.OnMonsterDestruction, _renderers);
            _renderers.SetRendererVisibility(false);
        }
    }
}
