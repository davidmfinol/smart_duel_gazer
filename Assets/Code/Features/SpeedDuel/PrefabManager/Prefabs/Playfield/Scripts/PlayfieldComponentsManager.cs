using Code.Core.Logger;
using Code.Features.SpeedDuel.EventHandlers;
using Code.Features.SpeedDuel.EventHandlers.Entities;
using Code.UI_Components.Constants;
using UnityEngine;
using Zenject;

namespace Code.Features.SpeedDuel.PrefabManager.Prefabs.Playfield.Scripts
{
    public class PlayfieldComponentsManager : MonoBehaviour
    {
        private const string Tag = "PlayfieldComponentsManager";
        
        private IPlayfieldEventHandler _playfieldEventHandler;
        private IAppLogger _logger;

        private Animator[] _animators;
        private MeshRenderer[] _renderers;

        #region Constructor

        [Inject]
        public void Construct (
            IPlayfieldEventHandler playfieldEventHandler,
            IAppLogger appLogger)
        {
            _playfieldEventHandler = playfieldEventHandler;
            _logger = appLogger;
        }

        #endregion

        #region LifeCycle

        private void Awake()
        {
            _animators = GetComponentsInChildren<Animator>();
            _renderers = GetComponentsInChildren<MeshRenderer>();

            _playfieldEventHandler.OnActivatePlayfield += OnActivatePlayfield;
            _playfieldEventHandler.OnAction += OnAction;
            _playfieldEventHandler.OnRemovePlayfield += OnRemovePlayfield;
        }

        private void OnDestroy()
        {
            _playfieldEventHandler.OnActivatePlayfield -= OnActivatePlayfield;
            _playfieldEventHandler.OnAction -= OnAction;
            _playfieldEventHandler.OnRemovePlayfield -= OnRemovePlayfield;
        }

        #endregion

        private void OnActivatePlayfield()
        {
            _logger.Log(Tag, "OnActivatePlayfield()");
            
            foreach(Animator animator in _animators)
            {
                animator.SetTrigger(AnimatorParameters.ActivatePlayfieldTrigger);
            }
        }

        private void OnAction(PlayfieldEvent playfieldEvent, PlayfieldEventArgs args)
        {
            _logger.Log(Tag, $"OnAction(PlayfieldEvent: {playfieldEvent})");
            
            switch (playfieldEvent)
            {
                case PlayfieldEvent.Rotate:
                    RotatePlayfield(args);
                    break;
                case PlayfieldEvent.Scale:
                    ScalePlayfield(args);
                    break;
                case PlayfieldEvent.Flip:
                    FlipPlayfield(args);
                    break;
                case PlayfieldEvent.Hide:
                    HidePlayfield(args);
                    break;
            }
        }

        private void OnRemovePlayfield()
        {
            _logger.Log(Tag, "OnRemovePlayfield()");
            
            foreach (Animator animator in _animators)
            {
                animator.SetTrigger(AnimatorParameters.RemovePlayfieldTrigger);
            }
        }

        #region Functions

        private void FlipPlayfield(PlayfieldEventArgs args)
        {
            _logger.Log(Tag, "FlipPlayfield()");
            
            if (!(args is PlayfieldEventValue<bool> state)) return;
            
            if (state.Value)
            {
                transform.localRotation = Quaternion.Euler(0, 180, 0);
                return;
            }

            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }

        private void HidePlayfield(PlayfieldEventArgs args)
        {
            _logger.Log(Tag, "HidePlayfield()");

            if (!(args is PlayfieldEventValue<bool> state)) return;

            foreach (MeshRenderer renderer in _renderers)
            {
                renderer.enabled = state.Value;
            }
        }

        private void RotatePlayfield(PlayfieldEventArgs args)
        {
            _logger.Log(Tag, "RotatePlayfield()");

            if (!(args is PlayfieldEventValue<float> rotation)) return;

            transform.rotation = Quaternion.Euler(0, rotation.Value, 0);
        }

        private void ScalePlayfield(PlayfieldEventArgs args)
        {
            _logger.Log(Tag, "RotatePlayfield()");

            if (!(args is PlayfieldEventValue<float> scale)) return;

            transform.localScale = new Vector3(scale.Value, scale.Value, scale.Value);
        }

        #endregion

        public class Factory : PlaceholderFactory<GameObject, PlayfieldComponentsManager>
        {
        }
    }
}