using Code.Features.SpeedDuel.EventHandlers.Entities;
using Code.Features.SpeedDuel.EventHandlers;
using Code.Core.Logger;
using UniRx;
using System;
using UnityEngine;
using Code.Features.SpeedDuel.Models;
using Code.Features.SpeedDuel.UseCases;
using Code.Core.DataManager;
using Code.Core.DataManager.GameObjects.Entities;

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

        private Subject<PlayfieldTransformValues> _activatePlayfieldMenu = new Subject<PlayfieldTransformValues>();
        public IObservable<PlayfieldTransformValues> ActivatePlayfieldMenu => _activatePlayfieldMenu;

        private Subject<bool> _togglePlayfieldMenu = new Subject<bool>();
        public IObservable<bool> TogglePlayfieldMenu => _togglePlayfieldMenu;

        private Subject<bool> _removePlayfield = new Subject<bool>();
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

            Init();
        }

        #endregion

        private void Init()
        {
            _logger.Log(Tag, "Init()");

            _playfieldEventHandler.OnActivatePlayfield += OnActivatePlayfield;
        }

        public void Dispose()
        {
            _logger.Log(Tag, "Dispose()");

            _playfieldEventHandler.OnActivatePlayfield -= OnActivatePlayfield;

            _activatePlayfieldMenu.Dispose();
            _togglePlayfieldMenu.Dispose();
            _removePlayfield.Dispose();
        }

        #region Playfield Events

        private void OnActivatePlayfield()
        {
            _logger.Log(Tag, "OnActivatePlayfield()");

            var playfield = _dataManager.Playfield;
            
            if (playfield == null) return;

            var playfieldValues = GetPlayfieldScaleAndRotation(playfield);            
            _activatePlayfieldMenu.OnNext(playfieldValues);
        }

        public void RotatePlayfield(PlayfieldEventArgs argsAsFloat)
        {
            _logger.Log(Tag, "RotatePlayfield()");
            
            if (!(argsAsFloat is PlayfieldEventValue<float>)) return;
            
            _playfieldEventHandler.Action(PlayfieldEvent.Rotate, argsAsFloat);
        }

        public void ScalePlayfield(PlayfieldEventArgs argsAsFloat)
        {
            _logger.Log(Tag, "ScalePlayfield()");

            if (!(argsAsFloat is PlayfieldEventValue<float>)) return;
            
            _playfieldEventHandler.Action(PlayfieldEvent.Scale, argsAsFloat);
        }

        public void HidePlayfield(PlayfieldEventArgs argsAsBool)
        {
            _logger.Log(Tag, "HidePlayfield()");

            if (!(argsAsBool is PlayfieldEventValue<bool>)) return;
            
            _playfieldEventHandler.Action(PlayfieldEvent.Hide, argsAsBool);
        }

        public void FlipPlayfield(PlayfieldEventArgs argsAsBool)
        {
            _logger.Log(Tag, "FlipPlayfield()");

            if (!(argsAsBool is PlayfieldEventValue<bool>)) return;

            _playfieldEventHandler.Action(PlayfieldEvent.Flip, argsAsBool);
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
            _endOfDuelUseCase.Execute();
        }

        #endregion

        private PlayfieldTransformValues GetPlayfieldScaleAndRotation(GameObject playfield)
        {            
            var scale = playfield.transform.localScale.x;
            var rotation = playfield.transform.localRotation.y;

            return new PlayfieldTransformValues { Scale = scale, YAxisRotation = rotation };
        }
    }
}