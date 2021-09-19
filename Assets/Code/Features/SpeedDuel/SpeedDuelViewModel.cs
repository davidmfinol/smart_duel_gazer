using Code.Core.Navigation;
using Code.Features.SpeedDuel.EventHandlers.Entities;
using Code.Features.SpeedDuel.EventHandlers;

namespace Code.Features.SpeedDuel
{
    public class SpeedDuelViewModel
    {
        private readonly INavigationService _navigationService;
        private IPlayfieldEventHandler _playfieldEventHandler;

        #region Constructor

        public SpeedDuelViewModel(
            INavigationService navigationService,
            IPlayfieldEventHandler playfieldEventHandler)
        {
            _navigationService = navigationService;
            _playfieldEventHandler = playfieldEventHandler;
        }

        #endregion

        #region Playfield Events

        public void RotatePlayfield(PlayfieldEventArgs args)
        {
            _playfieldEventHandler.Action(PlayfieldEvent.Rotate, args);
        }

        public void ScalePlayfield(PlayfieldEventArgs args)
        {
            _playfieldEventHandler.Action(PlayfieldEvent.Scale, args);
        }

        public void HidePlayfield(PlayfieldEventArgs args)
        {
            _playfieldEventHandler.Action(PlayfieldEvent.Hide, args);
        }

        public void FlipPlayfield(PlayfieldEventArgs args)
        {
            _playfieldEventHandler.Action(PlayfieldEvent.Flip, args);
        }

        #endregion

        public void OnBackButtonPressed()
        {
            _navigationService.ShowConnectionScene();
        }
    }
}