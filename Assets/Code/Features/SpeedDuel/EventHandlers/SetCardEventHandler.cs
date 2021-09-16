using Code.Features.SpeedDuel.EventHandlers.Entities;
using System;

namespace Code.Features.SpeedDuel.EventHandlers
{
    public interface ISetCardEventHandler
    {
        event Action<SetCardEvent, int> OnAction;
        event Action<int> OnSetCardRemove;
        event Action<int, string, bool> OnSummonSetCard;

        void Action(SetCardEvent eventName, int instanceID);
        void Remove(int instanceID);
        void Summon(int instanceID, string modelName, bool isMonster);
    }

    public class SetCardEventHandler : ISetCardEventHandler
    {
        private event Action<int, string, bool> _OnSummonSetCard;
        private event Action<SetCardEvent, int> _OnAction;
        private event Action<int> _OnSetCardRemove;

        #region Event Accessors

        public event Action<int, string, bool> OnSummonSetCard
        {
            add => _OnSummonSetCard += value;
            remove => _OnSummonSetCard -= value;
        }
        public event Action<SetCardEvent, int> OnAction
        {
            add => _OnAction += value;
            remove => _OnAction -= value;
        }
        public event Action<int> OnSetCardRemove
        {
            add => _OnSetCardRemove += value;
            remove => _OnSetCardRemove -= value;
        }

        #endregion

        public void Summon(int instanceID, string modelName, bool isMonster)
        {
            _OnSummonSetCard?.Invoke(instanceID, modelName, isMonster);
        }

        public void Action(SetCardEvent eventName, int instanceID)
        {
            _OnAction?.Invoke(eventName, instanceID);
        }

        public void Remove(int instanceID)
        {
            _OnSetCardRemove?.Invoke(instanceID);
        }
    }
}
