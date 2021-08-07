using Code.Features.SpeedDuel.EventHandlers;
using Code.Features.SpeedDuel.EventHandlers.Entities;
using Code.UI_Components.Constants;
using UnityEngine;
using Zenject;

namespace Code.Features.SpeedDuel.PrefabManager.Prefabs.Playfield.Scripts
{
    public class PlayfieldComponentsManager : MonoBehaviour
    {
        private PlayfieldEventHandler _playfieldEventHandler;

        private Animator[] _animators;
        private MeshRenderer[] _renderers;

        #region Constructor

        [Inject]
        public void Construct (
            PlayfieldEventHandler playfieldEventHandler)
        {
            _playfieldEventHandler = playfieldEventHandler;
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
            foreach(Animator animator in _animators)
            {
                animator.SetTrigger(AnimatorParameters.ActivatePlayfieldTrigger);
            }
        }

        private void OnAction(PlayfieldEvent playfieldEvent, PlayfieldEventArgs args)
        {
            switch (playfieldEvent)
            {
                case PlayfieldEvent.Rotate:
                    RotatePlayfield(args.floatValue);
                    break;
                case PlayfieldEvent.Scale:
                    ScalePlayfield(args.floatValue);
                    break;
                case PlayfieldEvent.Flip:
                    FlipPlayfield(args.boolValue);
                    break;
                case PlayfieldEvent.Hide:
                    HidePlayfield(args.boolValue);
                    break;
            }
        }

        private void OnRemovePlayfield()
        {
            foreach (Animator animator in _animators)
            {
                animator.SetTrigger(AnimatorParameters.RemovePlayfieldTrigger);
            }
        }

        #region Functions

        private void FlipPlayfield(bool state)
        {
            if (state)
            {
                transform.localRotation = Quaternion.Euler(0, 180, 0);
                return;
            }

            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }

        private void HidePlayfield(bool value)
        {
            foreach (MeshRenderer renderer in _renderers)
            {
                renderer.enabled = value;
            }
        }

        private void RotatePlayfield(float rotation)
        {
            transform.rotation = Quaternion.Euler(0, rotation, 0);
        }

        private void ScalePlayfield(float scale)
        {
            transform.localScale = new Vector3(scale, scale, scale);
        }

        #endregion

        public class Factory : PlaceholderFactory<GameObject, PlayfieldComponentsManager>
        {
        }
    }
}