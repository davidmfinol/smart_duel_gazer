using System;
using UnityEngine;
using AssemblyCSharp.Assets.Code.Core.Models.Interface.ModelEventsHandler.Entities;

namespace AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler
{
    public class ModelEventHandler : IModelEventHandler
    {
        //Set public events
        public event Action<string> OnRevealSetMonster;
        public event Action<string> OnDestroySetMonster;
        public event Action<string, string> OnSummonSpellTrap;
        public event Action<string> OnSpellTrapActivate;
        public event Action<string> OnSpellTrapRemove;
        
        private event Action<string> _OnSummonMonster;
        private event Action<string> _OnDestroyMonster;
        private event Action<string, bool> _OnChangeMonsterVisibility;
        private event Action<SkinnedMeshRenderer[]> _OnMonsterRemoval;

        #region Event Accessors

        public event Action<string> OnSummonMonster 
        { 
            add => _OnSummonMonster += value;
            remove => _OnSummonMonster -= value;
        }
        public event Action<string> OnDestroyMonster 
        { 
            add => _OnDestroyMonster += value;
            remove => _OnDestroyMonster -= value;
        }
        public event Action<string, bool> OnChangeMonsterVisibility 
        {
            add => _OnChangeMonsterVisibility += value;
            remove => _OnChangeMonsterVisibility -= value;
        }
        public event Action<SkinnedMeshRenderer[]> OnMonsterRemoval 
        { 
            add => _OnMonsterRemoval += value;
            remove => _OnMonsterRemoval -= value;
        }

        #endregion

        public void RaiseEvent(EventNames eventName, string zone)
        {
            switch (eventName)
            {
                case EventNames.SummonMonster:
                    _OnSummonMonster?.Invoke(zone);
                    break;
                case EventNames.DestroyMonster:
                    _OnDestroyMonster?.Invoke(zone);
                    break;
                case EventNames.RevealSetMonster:
                    OnRevealSetMonster?.Invoke(zone);
                    break;
                case EventNames.DestroySetMonster:
                    OnDestroySetMonster?.Invoke(zone);
                    break;
                case EventNames.SpellTrapActivate:
                    OnSpellTrapActivate?.Invoke(zone);
                    break;
                case EventNames.SpellTrapRemove:
                    OnSpellTrapRemove?.Invoke(zone);
                    break;
            }
        }
        public void RaiseEvent(EventNames eventName, string zone, string modelName)
        {
            if (eventName != EventNames.SummonSpellTrap)
            {
                return;
            }
            OnSummonSpellTrap?.Invoke(zone, modelName);
        }
        
        public void RaiseEvent(EventNames eventName, string zone, bool state)
        {
            if (eventName != EventNames.ChangeMonsterVisibility)
            {
                Debug.LogWarning($"{eventName} does not exist in this context");
                return;
            }
            _OnChangeMonsterVisibility?.Invoke(zone, state);
        }

        public void RaiseEvent(EventNames eventName, SkinnedMeshRenderer[] renderers)
        {
            if (eventName != EventNames.MonsterDestruction)
            {
                Debug.LogWarning($"{eventName} does not exist in this context");
                return;
            }
            _OnMonsterRemoval?.Invoke(renderers);
        }
    }
}
