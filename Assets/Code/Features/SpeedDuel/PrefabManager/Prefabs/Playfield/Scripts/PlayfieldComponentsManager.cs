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
        private const string ZoneMaterialName = "Zone Material";
        private const float ZoneInitialTransparency = 54f;
        private const float MatInitialTransparency = 47f;

        private IPlayfieldEventHandler _playfieldEventHandler;
        private IAppLogger _logger;

        private Animator[] _animators;
        private MeshRenderer[] _renderers;

        #region Constructor

        [Inject]
        public void Construct(
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

            foreach (var animator in _animators)
            {
                animator.SetTrigger(AnimatorParameters.ActivatePlayfieldTrigger);
            }
        }

        private void OnAction(PlayfieldEvent playfieldEvent, PlayfieldEventArgs args)
        {
            _logger.Log(Tag, $"OnAction(playfieldEvent: {playfieldEvent})");

            switch (playfieldEvent)
            {
                case PlayfieldEvent.Transparency:
                    ChangeTransparency(args);
                    break;
                case PlayfieldEvent.Scale:
                    ScalePlayfield(args);
                    break;
                default:
                    _logger.Log(Tag, $"Unexpected action: {playfieldEvent}");
                    break;
            }
        }

        private void OnRemovePlayfield()
        {
            _logger.Log(Tag, "OnRemovePlayfield()");

            foreach (var animator in _animators)
            {
                animator.SetTrigger(AnimatorParameters.RemovePlayfieldTrigger);
            }
        }

        private void ChangeTransparency(PlayfieldEventArgs args)
        {
            _logger.Log(Tag, $"ChangeTransparency(args: {args})");

            if (!(args is PlayfieldEventValue<float> transparency)) return;

            foreach (var renderer in _renderers)
            {
                // Change from material's Opacity (0-255) to an Alpha Value (0-1). Clamp to avoid negative values
                var material = renderer.material;
                var newTransparency = material.name == ZoneMaterialName
                    ? Mathf.Clamp(transparency.Value - ZoneInitialTransparency, 0, 255) / 255
                    : Mathf.Clamp(transparency.Value - MatInitialTransparency, 0, 255) / 255;

                material.color = new Color(material.color.r, material.color.g, material.color.b, newTransparency);
            }
        }

        private void ScalePlayfield(PlayfieldEventArgs args)
        {
            _logger.Log(Tag, $"ScalePlayfield(args: {args})");

            if (!(args is PlayfieldEventValue<float> scale)) return;

            transform.localScale = new Vector3(scale.Value, scale.Value, scale.Value);
        }

        public class Factory : PlaceholderFactory<GameObject, PlayfieldComponentsManager>
        {
        }
    }
}