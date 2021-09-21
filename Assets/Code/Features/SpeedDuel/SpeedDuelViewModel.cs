using Code.Core.Navigation;
using Code.Features.SpeedDuel.EventHandlers.Entities;
using Code.Features.SpeedDuel.EventHandlers;
using Code.Core.Logger;
using UniRx;
using System;

namespace Code.Features.SpeedDuel
{
    public class SpeedDuelViewModel
    {
        private const string Tag = "Speed Duel View Model";

        private readonly INavigationService _navigationService;
        private readonly IPlayfieldEventHandler _playfieldEventHandler;
        private readonly IAppLogger _logger;

        private BehaviorSubject<bool> _togglePlayfieldMenu = new BehaviorSubject<bool>(default);
        private BehaviorSubject<bool> _removePlayfield = new BehaviorSubject<bool>(default);

        public IObservable<bool> TogglePlayfieldMenu => _togglePlayfieldMenu;
        public IObservable<bool> RemovePlayfield => _removePlayfield;

        #region Constructor

        public SpeedDuelViewModel(
            INavigationService navigationService,
            IPlayfieldEventHandler playfieldEventHandler,
            IAppLogger appLogger)
        {
            _navigationService = navigationService;
            _playfieldEventHandler = playfieldEventHandler;
            _logger = appLogger;
        }

        #endregion

        #region Playfield Events

        public void RotatePlayfield(PlayfieldEventArgs floatValue)
        {
            _logger.Log(Tag, $"Rotate Playfield with value: {floatValue}");
            
            if (!(floatValue is PlayfieldEventValue<float>)) return;
            
            _playfieldEventHandler.Action(PlayfieldEvent.Rotate, floatValue);
        }

        public void ScalePlayfield(PlayfieldEventArgs floatValue)
        {
            _logger.Log(Tag, $"Scale Playfield with value: {floatValue}");

            if (!(floatValue is PlayfieldEventValue<float>)) return;
            
            _playfieldEventHandler.Action(PlayfieldEvent.Scale, floatValue);
        }

        public void HidePlayfield(PlayfieldEventArgs boolValue)
        {
            _logger.Log(Tag, $"Hide Playfield with value: {boolValue}");

            if (!(boolValue is PlayfieldEventValue<bool>)) return;
            
            _playfieldEventHandler.Action(PlayfieldEvent.Hide, boolValue);
        }

        public void FlipPlayfield(PlayfieldEventArgs boolValue)
        {
            _logger.Log(Tag, $"Flip Playfield with value: {boolValue}");

            if (!(boolValue is PlayfieldEventValue<bool>)) return;

            _playfieldEventHandler.Action(PlayfieldEvent.Flip, boolValue);
        }

        #endregion

        #region UI Events

        public void OnTogglePlayfieldMenu(bool state)
        {
            _togglePlayfieldMenu.OnNext(state);
        }
        
        public void OnRemovePlayfield()
        {
            _removePlayfield.OnNext(true);
            _playfieldEventHandler.RemovePlayfield();            
        }

        public void OnBackButtonPressed()
        {
            _navigationService.ShowConnectionScene();
        }

        #endregion
    }
}