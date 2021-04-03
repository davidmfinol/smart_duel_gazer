using System;
using UnityEngine;
using AssemblyCSharp.Assets.Code.Core.Models.Interface.ModelEventsHandler.Entities;

namespace AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler
{
    public class ModelEventHandler : IModelEventHandler
    {
        public event Action<string> OnSummonMonster;
        public event Action<string> OnDestroyMonster;
        public event Action<string> OnDestroySetMonster;

        public event Action<string, string> OnSummonSpellTrap;
        public event Action<string> OnSpellTrapActivate;
        public event Action<string> OnSpellTrapRemove;

        public event Action<string, bool> OnChangeMonsterVisibility;
        public event Action<SkinnedMeshRenderer[]> OnMonsterDestruction;

        public void RaiseEvent(EventNames eventName, string zone)
        {
            switch (eventName)
            {
                case EventNames.SummonMonster:
                    OnSummonMonster?.Invoke(zone);
                    break;
                case EventNames.DestroyMonster:
                    OnDestroyMonster?.Invoke(zone);
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
            if(eventName != EventNames.ChangeMonsterVisibility)
            {
                return;
            }
            OnChangeMonsterVisibility?.Invoke(zone, state);
        }
        public void RaiseEvent(EventNames eventName, SkinnedMeshRenderer[] renderers)
        {
            if (eventName != EventNames.MonsterDestruction)
            {
                Debug.LogWarning($"{eventName} does not exist in this context");
                return;
            }
            OnMonsterDestruction?.Invoke(renderers);
        }
    }
}
