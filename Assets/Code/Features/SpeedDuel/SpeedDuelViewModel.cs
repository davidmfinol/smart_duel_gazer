using Code.Core.Navigation;
using Code.Features.SpeedDuel.EventHandlers.Entities;
using Code.Features.SpeedDuel.EventHandlers;
using Code.Core.Logger;
using UniRx;
using System;
using UnityEngine;
using Code.Features.SpeedDuel.Models;

namespace Code.Features.SpeedDuel
{
    public class SpeedDuelViewModel
    {
        private const string Tag = "Speed Duel View Model";

        private readonly INavigationService _navigationService;
        private readonly IPlayfieldEventHandler _playfieldEventHandler;
        private readonly IAppLogger _logger;

        private Subject<PlayfieldTransformValues> _activatePlayfieldMenu = new Subject<PlayfieldTransformValues>();
        private Subject<bool> _togglePlayfieldMenu = new Subject<bool>();
        private Subject<bool> _removePlayfield = new Subject<bool>();

        #region Observables

        public IObservable<PlayfieldTransformValues> ActivatePlayfieldMenu => _activatePlayfieldMenu;
        public IObservable<bool> TogglePlayfieldMenu => _togglePlayfieldMenu;
        public IObservable<bool> RemovePlayfield => _removePlayfield;

        #endregion

        #region Constructor

        public SpeedDuelViewModel(
            INavigationService navigationService,
            IPlayfieldEventHandler playfieldEventHandler,
            IAppLogger appLogger)
        {
            _navigationService = navigationService;
            _playfieldEventHandler = playfieldEventHandler;
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
            _playfieldEventHandler.OnActivatePlayfield -= OnActivatePlayfield;
        }

        #region Playfield Events

        private void OnActivatePlayfield(UnityEngine.GameObject playfield)
        {
            _logger.Log(Tag, "OnActivatePlayfield()");
            
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
            _navigationService.ShowConnectionScene();
        }

        #endregion

        private PlayfieldTransformValues GetPlayfieldScaleAndRotation(GameObject playfield)
        {            
            var scale = playfield.transform.localScale.x;
            var rotation = playfield.transform.localRotation.y;

            return new PlayfieldTransformValues { Scale = scale, Rotation = rotation };
        }
    }
}