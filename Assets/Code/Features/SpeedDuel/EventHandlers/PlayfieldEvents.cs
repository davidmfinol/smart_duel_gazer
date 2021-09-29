using Code.Features.SpeedDuel.EventHandlers.Entities;
using System;

namespace Code.Features.SpeedDuel.EventHandlers
{
    public interface IPlayfieldEventHandler
    {
        event Action<PlayfieldEvent, PlayfieldEventArgs> OnAction;
        event Action<UnityEngine.GameObject> OnActivatePlayfield;
        event Action OnRemovePlayfield;

        void Action(PlayfieldEvent playfieldEvent, PlayfieldEventArgs args);
        void ActivatePlayfield(UnityEngine.GameObject playfield);
        void RemovePlayfield();
    }

    public class PlayfieldEventHandler : IPlayfieldEventHandler
    {
        private event Action<UnityEngine.GameObject> _OnActivatePlayfield;
        private event Action<PlayfieldEvent, PlayfieldEventArgs> _OnAction;
        private event Action _OnRemovePlayfield;

        #region Event Accessors

        public event Action<UnityEngine.GameObject> OnActivatePlayfield
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

        public void ActivatePlayfield(UnityEngine.GameObject playfield)
        {
            _OnActivatePlayfield?.Invoke(playfield);
        }

        public void Action(PlayfieldEvent playfieldEvent, PlayfieldEventArgs args)
        {
            _OnAction?.Invoke(playfieldEvent, args);
        }

        public void RemovePlayfield()
        {
            _OnRemovePlayfield?.Invoke();
        }
    }
}
