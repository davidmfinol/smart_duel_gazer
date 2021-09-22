using Code.Features.SpeedDuel.EventHandlers.Entities;
using System;
using UnityEngine;

namespace Code.Features.SpeedDuel.EventHandlers
{
    public interface IModelEventHandler
    {
        event Action<ModelEvent, int, bool> OnAction;
        event Action<int> OnActivateModel;
        event Action<int, Transform> OnDirectAttack;
        event Action<SkinnedMeshRenderer[]> OnMonsterRemoval;
        event Action<int> OnRemove;
        event Action<int> OnSummon;

        void Action(ModelEvent eventName, int instanceID, bool state = true);
        void Activate(int instanceID);
        void RaiseMonsterRemovalEvent(SkinnedMeshRenderer[] renderers);
        public void RaiseDirectAttack(int instanceId, Transform targetZone);
        void Remove(int instanceID);
        void Summon(int instanceID);
    }

    public class ModelEventHandler : IModelEventHandler
    {
        private event Action<int> _OnActivateModel;
        private event Action<int> _OnSummon;
        private event Action<ModelEvent, int, bool> _OnAction;
        private event Action<int, Transform> _OnDirectAttack;
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

        public event Action<ModelEvent, int, bool> OnAction
        {
            add => _OnAction += value;
            remove => _OnAction -= value;
        }

        public event Action<int, Transform> OnDirectAttack
        {
            add => _OnDirectAttack += value;
            remove => _OnDirectAttack -= value;
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

        public void Action(ModelEvent eventName, int instanceID, bool state = true)
        {
            _OnAction?.Invoke(eventName, instanceID, state);
        }

        public void RaiseDirectAttack(int instanceId, Transform targetZone)
        {
            _OnDirectAttack?.Invoke(instanceId, targetZone);
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