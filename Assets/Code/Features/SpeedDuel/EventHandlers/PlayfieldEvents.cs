using Code.Features.SpeedDuel.EventHandlers.Entities;
using System;

namespace Code.Features.SpeedDuel.EventHandlers
{
    public interface IPlayfieldEventHandler
    {
        event Action<PlayfieldEvent, PlayfieldEventArgs> OnAction;
        event Action OnActivatePlayfield;
        event Action OnRemovePlayfield;

        bool IsSettingsMenuActive { get; }

        void Action(PlayfieldEvent playfieldEvent, PlayfieldEventArgs args);
        void ActivatePlayfield();
        void RemovePlayfield();
        void SetSettingsMenuState(bool state);
    }

    public class PlayfieldEventHandler : IPlayfieldEventHandler
    {
        private event Action _OnActivatePlayfield;
        private event Action<PlayfieldEvent, PlayfieldEventArgs> _OnAction;
        private event Action _OnRemovePlayfield;

        private bool _isSettingsMenuActive;
        public bool IsSettingsMenuActive => _isSettingsMenuActive;

        #region Event Accessors

        public event Action OnActivatePlayfield
        {
            add => _OnActivatePlayfield += value;
            remove => _OnActivatePlayfield -= value;
        }
        public event Action<PlayfieldEvent, PlayfieldEventArgs> OnAction
        {
            add => _OnAction += value;
            remove => _OnAction -= value;
        }
        public event Action OnRemovePlayfield
        {
            add => _OnRemovePlayfield += value;
            remove => _OnRemovePlayfield -= value;
        }

        #endregion

        public void ActivatePlayfield()
        {
            _OnActivatePlayfield?.Invoke();
        }

        public void Action(PlayfieldEvent playfieldEvent, PlayfieldEventArgs args)
        {
            _OnAction?.Invoke(playfieldEvent, args);
        }

        public void RemovePlayfield()
        {
            _OnRemovePlayfield?.Invoke();
        }

        public void SetSettingsMenuState(bool state)
        {
            _isSettingsMenuActive = state;
        }
    }
}
