using Code.Features.SpeedDuel.EventHandlers.Entities;
using Code.Features.SpeedDuel.EventHandlers;
using Code.Core.Logger;
using UniRx;
using System;
using UnityEngine;
using Code.Features.SpeedDuel.Models;
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

        private readonly Subject<PlayfieldTransformValues> _activatePlayfieldMenu = new Subject<PlayfieldTransformValues>();
        public IObservable<PlayfieldTransformValues> ActivatePlayfieldMenu => _activatePlayfieldMenu;

        private readonly Subject<bool> _playfieldMenuVisibility = new Subject<bool>();
        public IObservable<bool> PlayfieldMenuVisibility => _playfieldMenuVisibility;

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

            _activatePlayfieldMenu.Dispose();
            _playfieldMenuVisibility.Dispose();
            _removePlayfield.Dispose();
        }

        #region Playfield Events

        private void OnActivatePlayfield()
        {
            _logger.Log(Tag, "OnActivatePlayfield()");

            var playfield = _dataManager.GetPlayfield();
            if (playfield == null) return;

            var playfieldValues = GetPlayfieldTransformValues(playfield);
            _activatePlayfieldMenu.OnNext(playfieldValues);
        }

        private PlayfieldTransformValues GetPlayfieldTransformValues(GameObject playfield)
        {
            _logger.Log(Tag, "GetPlayfieldTransformValues()");

            var scale = playfield.transform.localScale.x;
            var rotation = playfield.transform.localRotation.y;

            return new PlayfieldTransformValues(scale, rotation);
        }

        public void UpdatePlayfieldRotation(float rotation)
        {
            _logger.Log(Tag, $"UpdatePlayfieldRotation(rotation: {rotation})");

            _playfieldEventHandler.Action(PlayfieldEvent.Rotate, new PlayfieldEventValue<float>(rotation));
        }

        public void UpdatePlayfieldScale(float scale)
        {
            _logger.Log(Tag, $"UpdatePlayfieldScale(scale: {scale})");

            _playfieldEventHandler.Action(PlayfieldEvent.Scale, new PlayfieldEventValue<float>(scale));
        }

        public void UpdatePlayfieldVisibility(bool showPlayfield)
        {
            _logger.Log(Tag, $"UpdatePlayfieldVisibility(showPlayfield: {showPlayfield})");

            _playfieldEventHandler.Action(PlayfieldEvent.Hide, new PlayfieldEventValue<bool>(showPlayfield));
        }

        public void FlipPlayfield(bool shouldFlip)
        {
            _logger.Log(Tag, $"FlipPlayfield(shouldFlip: {shouldFlip})");

            _playfieldEventHandler.Action(PlayfieldEvent.Flip, new PlayfieldEventValue<bool>(shouldFlip));
        }

        #endregion

        #region UI Events

        public void OnTogglePlayfieldMenu(bool showPlayfield)
        {
            _logger.Log(Tag, $"OnTogglePlayfieldMenu(showPlayfield: {showPlayfield})");

            _playfieldMenuVisibility.OnNext(showPlayfield);
        }

        public void OnRemovePlayfield()
        {
            _logger.Log(Tag, "OnRemovePlayfield()");

            _removePlayfield.OnNext(true);
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