using Code.Features.SpeedDuel.EventHandlers.Entities;
using Code.Features.SpeedDuel.EventHandlers;
using Code.Core.Logger;
using UniRx;
using System;
using Code.Features.SpeedDuel.UseCases;
using Code.Core.DataManager;

namespace Code.Features.SpeedDuel
{
    public class SpeedDuelViewModel
    {
        private const string Tag = "SpeedDuelViewModel";

        private readonly IPlayfieldEventHandler _playfieldEventHandler;
        private readonly IDataManager _dataManager;
        private readonly IEndOfDuelUseCase _endOfDuelUseCase;
        private readonly IAppLogger _logger;

        #region Properties

        private readonly BehaviorSubject<bool> _showSettingsMenu = new BehaviorSubject<bool>(false);
        public IObservable<bool> ShowSettingsMenu => _showSettingsMenu;

        private readonly Subject<float> _activatePlayfieldUIElements = new Subject<float>();
        public IObservable<float> ActivatePlayfieldUIElements => _activatePlayfieldUIElements;

        private readonly Subject<bool> _removePlayfield = new Subject<bool>();
        public IObservable<bool> RemovePlayfield => _removePlayfield;

        #endregion

        #region Constructor

        public SpeedDuelViewModel(
            IPlayfieldEventHandler playfieldEventHandler,
            IDataManager dataManager,
            IEndOfDuelUseCase endOfDuelUseCase,
            IAppLogger appLogger)
        {
            _playfieldEventHandler = playfieldEventHandler;
            _dataManager = dataManager;
            _endOfDuelUseCase = endOfDuelUseCase;
            _logger = appLogger;
        }

        #endregion

        public void Init()
        {
            _logger.Log(Tag, "Init()");

            _playfieldEventHandler.OnActivatePlayfield += OnActivatePlayfield;
        }

        public void Dispose()
        {
            _logger.Log(Tag, "Dispose()");

            _playfieldEventHandler.OnActivatePlayfield -= OnActivatePlayfield;

            _activatePlayfieldUIElements.Dispose();
            _removePlayfield.Dispose();
        }

        #region Playfield Events

        private void OnActivatePlayfield()
        {
            _logger.Log(Tag, "OnActivatePlayfield()");

            var playfield = _dataManager.GetPlayfield();
            if (playfield == null) return;

            var playfieldScale = playfield.transform.localScale.x;
            _activatePlayfieldUIElements.OnNext(playfieldScale);
        }

        public void UpdatePlayfieldTransparency(float transparency)
        {
            _logger.Log(Tag, $"UpdatePlayfieldTransparency(transparency: {transparency})");

            _playfieldEventHandler.Action(PlayfieldEvent.Transparency, new PlayfieldEventValue<float>(transparency));
        }

        public void UpdatePlayfieldScale(float scale)
        {
            _logger.Log(Tag, $"UpdatePlayfieldScale(scale: {scale})");

            _playfieldEventHandler.Action(PlayfieldEvent.Scale, new PlayfieldEventValue<float>(scale));
        }

        #endregion

        #region UI Events

        public void OnToggleSettingsMenu(bool showMenu)
        {
            _logger.Log(Tag, $"OnToggleSettingsMenu(showMenu: {showMenu})");

            _showSettingsMenu.OnNext(showMenu);
            _playfieldEventHandler.SetSettingsMenuState(showMenu);
        }

        public void OnRemovePlayfield()
        {
            _logger.Log(Tag, "OnRemovePlayfield()");

            _removePlayfield.OnNext(false);
            _playfieldEventHandler.RemovePlayfield();
        }

        public void OnBackButtonPressed()
        {
            _logger.Log(Tag, "OnBackButtonPressed()");

            _endOfDuelUseCase.Execute();
        }

        #endregion
    }
}