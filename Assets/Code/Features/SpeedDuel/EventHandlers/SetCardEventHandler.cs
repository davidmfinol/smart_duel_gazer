using Code.Features.SpeedDuel.EventHandlers.Entities;
using System;

namespace Code.Features.SpeedDuel.EventHandlers
{
    public interface ISetCardEventHandler
    {
        public void RaiseEventByEventName(SetCardEvent eventName, int instanceID);
        public void RaiseSummonSetCardEvent(int instanceID, string modelName, bool isMonster);
    }

    public class SetCardEventHandler : ISetCardEventHandler
    {
        private event Action<int> _OnShowSetCard;
        private event Action<int> _OnHideSetMonster;
        private event Action<int> _OnDestroySetMonster;

        private event Action<int> _OnSpellTrapActivate;
        private event Action<int> _OnReturnToFaceDown;
        private event Action<int> _OnSetCardRemove;

        private event Action<int, string, bool> _OnSummonSetCard;

        #region Event Accessors

        public event Action<int> OnShowSetCard
        {
            add => _OnShowSetCard += value;
            remove => _OnShowSetCard -= value;
        }        
        public event Action<int> OnHideSetMonster
        {
            add => _OnHideSetMonster += value;
            remove => _OnHideSetMonster -= value;
        }
        public event Action<int> OnDestroySetMonster
        {
            add => _OnDestroySetMonster += value;
            remove => _OnDestroySetMonster -= value;
        }
        public event Action<int> OnSpellTrapActivate
        {
            add => _OnSpellTrapActivate += value;
            remove => _OnSpellTrapActivate -= value;
        }
        public event Action<int> OnReturnToFaceDown
        {
            add => _OnReturnToFaceDown += value;
            remove => _OnReturnToFaceDown -= value;
        }
        public event Action<int> OnSetCardRemove
        {
            add => _OnSetCardRemove += value;
            remove => _OnSetCardRemove -= value;
        }
        public event Action<int, string, bool> OnSummonSetCard
        {
            add => _OnSummonSetCard += value;
            remove => _OnSummonSetCard -= value;
        }

        #endregion

        public void RaiseEventByEventName(SetCardEvent eventName, int instanceID)
        {
            switch (eventName)
            {
                case SetCardEvent.ShowSetCard:
                    _OnShowSetCard?.Invoke(instanceID);
                    break;
                case SetCardEvent.HideSetMonster:
                    _OnHideSetMonster?.Invoke(instanceID);
                    break;
                case SetCardEvent.DestroySetMonster:
                    _OnDestroySetMonster?.Invoke(instanceID);
                    break;
                case SetCardEvent.SpellTrapActivate:
                    _OnSpellTrapActivate?.Invoke(instanceID);
                    break;
                case SetCardEvent.SetCardRemove:
                    _OnSetCardRemove?.Invoke(instanceID);
                    break;
                case SetCardEvent.ReturnToFaceDown:
                    _OnReturnToFaceDown?.Invoke(instanceID);
                    break;
            }
        }

        public void RaiseSummonSetCardEvent(int instanceID, string modelName, bool isMonster)
        {
            _OnSummonSetCard?.Invoke(instanceID, modelName, isMonster);
        }
    }
}
