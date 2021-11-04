using Code.Features.SpeedDuel.EventHandlers.Entities;
using System;
using UnityEngine;

namespace Code.Features.SpeedDuel.EventHandlers
{
    public interface IModelEventHandler
    {
        event Action<ModelEvent, int, ModelActionEventArgs> OnAction;
        event Action<int> OnActivateModel;
        event Action<SkinnedMeshRenderer[]> OnMonsterRemoval;
        event Action<int> OnRemove;
        event Action<int> OnSummon;

        void Action(ModelEvent eventName, int instanceID, ModelActionEventArgs args);
        void Activate(int instanceID);
        void RaiseMonsterRemovalEvent(SkinnedMeshRenderer[] renderers);
        void Remove(int instanceID);
        void Summon(int instanceID);
    }

    public class ModelEventHandler : IModelEventHandler
    {
        private event Action<int> _OnActivateModel;
        private event Action<int> _OnSummon;
        private event Action<ModelEvent, int, ModelActionEventArgs> _OnAction;
        private event Action<int> _OnRemove;

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

        public event Action<ModelEvent, int, ModelActionEventArgs> OnAction
        {
            add => _OnAction += value;
            remove => _OnAction -= value;
        }

        public event Action<int> OnRemove
        {
            add => _OnRemove += value;
            remove => _OnRemove -= value;
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

        public void Action(ModelEvent eventName, int instanceID, ModelActionEventArgs args)
        {            
            _OnAction?.Invoke(eventName, instanceID, args);
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