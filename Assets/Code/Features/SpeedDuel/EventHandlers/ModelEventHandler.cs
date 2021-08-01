using Code.Features.SpeedDuel.EventHandlers.Entities;
using System;
using UnityEngine;

namespace Code.Features.SpeedDuel.EventHandlers
{
    //public interface IModelEventHandler
    //{       
    //    public void Activate(int instanceID);
    //    public void RaiseEventByEventName(ModelEvent eventNames, int instanceID);
    //    public void RaiseChangeVisibilityEvent(int instanceID, bool state);
    //    public void RaiseMonsterRemovalEvent(SkinnedMeshRenderer[] renderers);
    //}

    public interface IModelEventHandler
    {
        public void Summon(int instanceID);
        public void Action(ModelEvent eventName, int instanceID);
        public void Remove(int instanceID);
    }

    public class ModelEventHandler : IModelEventHandler
    {
        private event Action<int> _OnActivateModel;
        private event Action<int> _OnSummon;
        private event Action<ModelEvent, int> _OnAction;
        private event Action<int> _OnRemove;

        private event Action<int, bool> _OnChangeMonsterVisibility;
        private event Action<SkinnedMeshRenderer[]> _OnMonsterRemoval;

        #region Event Accessors

        public event Action<int> OnActivateModel
        {
            add => _OnActivateModel += value;
            remove => _OnActivateModel -= value;
        }
        public event Action<int> OnSummon 
        { 
            add => _OnSummon += value;
            remove => _OnSummon -= value;
        }
        public event Action<ModelEvent, int> OnAction
        {
            add => _OnAction += value;
            remove => _OnAction -= value;
        }
        public event Action<int> OnRemove
        {
            add => _OnRemove += value;
            remove => _OnRemove -= value;
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

        public void Summon(int instanceID)
        {
            _OnSummon?.Invoke(instanceID);
        }
        
        public void Action(ModelEvent eventName, int instanceID)
        {
            _OnAction?.Invoke(eventName, instanceID);
        }
        
        public void RaiseChangeVisibilityEvent(int instanceID, bool state)
        {
            _OnChangeMonsterVisibility?.Invoke(instanceID, state);
        }

        public void RaiseMonsterRemovalEvent(SkinnedMeshRenderer[] renderers)
        {
            _OnMonsterRemoval?.Invoke(renderers);
        }

        public void Remove(int instanceID)
        {
            _OnRemove?.Invoke(instanceID);
        }
    }
}
