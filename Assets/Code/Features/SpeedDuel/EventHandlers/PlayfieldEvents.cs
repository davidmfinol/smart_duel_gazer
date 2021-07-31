using System;

namespace Code.Features.SpeedDuel.EventHandlers
{
    public interface IPlayfieldEventHandler
    {
        public void ActivatePlayfield(UnityEngine.GameObject playfield);
        public void PickupPlayfield();
        public void DestroyPlayfield();
    }

    public class PlayfieldEventHandler : IPlayfieldEventHandler
    {
        private event Action<UnityEngine.GameObject> _OnActivatePlayfield;
        private event Action _OnPickUpPlayfield;
        private event Action _OnDestroyPlayfield;

        #region Event Accessors

        public event Action<UnityEngine.GameObject> OnActivatePlayfield
        {
            add => _OnActivatePlayfield += value;
            remove => _OnActivatePlayfield -= value;
        }
        public event Action OnPickupPlayfield
        {
            add => _OnPickUpPlayfield += value;
            remove => _OnPickUpPlayfield -= value;
        }
        public event Action OnDestroyPlayfield
        {
            add => _OnDestroyPlayfield += value;
            remove => _OnDestroyPlayfield -= value;
        }

        #endregion

        public void ActivatePlayfield(UnityEngine.GameObject playfield)
        {
            _OnActivatePlayfield?.Invoke(playfield);
        }

        public void PickupPlayfield()
        {
            _OnPickUpPlayfield?.Invoke();
        }

        public void DestroyPlayfield()
        {
            _OnDestroyPlayfield?.Invoke();
        }
    }
}
