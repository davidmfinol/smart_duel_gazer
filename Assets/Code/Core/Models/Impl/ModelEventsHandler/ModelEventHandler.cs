using System;
using UnityEngine;
using AssemblyCSharp.Assets.Code.Core.Models.Interface.ModelEventsHandler.Entities;

namespace AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler
{
    public class ModelEventHandler : IModelEventHandler
    {
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
            }
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
            if (eventName != EventNames.OnMonsterDestruction)
            {
                Debug.LogWarning($"{eventName} does not exist in this context");
                return;
            }
            _OnMonsterRemoval?.Invoke(renderers);
        }
    }
}
