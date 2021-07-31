using Code.Features.SpeedDuel.EventHandlers.Entities;
using System;
using UnityEngine;

namespace Code.Features.SpeedDuel.EventHandlers
{
    public interface IModelEventHandler
    {       
        public void Activate(int instanceID);
        public void RaiseEventByEventName(ModelEvent eventNames, int instanceID);
        public void RaiseChangeVisibilityEvent(int instanceID, bool state);
        public void RaiseMonsterRemovalEvent(SkinnedMeshRenderer[] renderers);
    }
    
    public class ModelEventHandler : IModelEventHandler
    {
        private event Action<int> _OnActivateModel;

        private event Action<int> _OnSummonMonster;
        private event Action<int> _OnDestroyMonster;
        private event Action<int> _OnAttack;
        private event Action<int> _OnRevealSetMonster;

        private event Action<int, bool> _OnChangeMonsterVisibility;
        private event Action<SkinnedMeshRenderer[]> _OnMonsterRemoval;

        #region Event Accessors

        public event Action<int> OnActivateModel
        {
            add => _OnActivateModel += value;
            remove => _OnActivateModel -= value;
        }
        public event Action<int> OnSummonMonster 
        { 
            add => _OnSummonMonster += value;
            remove => _OnSummonMonster -= value;
        }
        public event Action<int> OnDestroyMonster 
        { 
            add => _OnDestroyMonster += value;
            remove => _OnDestroyMonster -= value;
        }
        public event Action<int> OnAttack
        {
            add => _OnAttack += value;
            remove => _OnAttack -= value;
        }
        public event Action<int> OnRevealSetMonster
        {
            add => _OnRevealSetMonster += value;
            remove => _OnRevealSetMonster -= value;
        }
        public event Action<int, bool> OnChangeMonsterVisibility 
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

        public void Activate(int instanceID)
        {
            _OnActivateModel?.Invoke(instanceID);
        }
        
        public void RaiseEventByEventName(ModelEvent eventName, int instanceID)
        {
            switch (eventName)
            {
                case ModelEvent.SummonMonster:
                    _OnSummonMonster?.Invoke(instanceID);
                    break;
                case ModelEvent.DestroyMonster:
                    _OnDestroyMonster?.Invoke(instanceID);
                    break;
                case ModelEvent.Attack:
                    _OnAttack?.Invoke(instanceID);
                    break;
                case ModelEvent.RevealSetMonster:
                    _OnRevealSetMonster?.Invoke(instanceID);
                    break;
            }
        }
        
        public void RaiseChangeVisibilityEvent(int instanceID, bool state)
        {
            _OnChangeMonsterVisibility?.Invoke(instanceID, state);
        }

        public void RaiseMonsterRemovalEvent(SkinnedMeshRenderer[] renderers)
        {
            _OnMonsterRemoval?.Invoke(renderers);
        }
    }
}
