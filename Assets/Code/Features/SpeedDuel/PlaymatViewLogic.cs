using Code.Core.Models.ModelEventsHandler;
using Code.UI_Components.Constants;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.Features.SpeedDuel
{
    public class PlaymatViewLogic : MonoBehaviour
    {
        [SerializeField] private GameObject _menus;
        [SerializeField] private Slider _scaleSlider;
        [SerializeField] private Slider _rotationSlider;
        [SerializeField] private Toggle _toggleView;

        private ModelEventHandler _modelEventHandler;

        private GameObject _playmatShell;
        private Animator _playmatAnimator;
        private Animator[] _animators;
        private MeshRenderer[] _renderers;

        #region Constructor

        [Inject]
        public void Construct(
            ModelEventHandler modelEventHandler)
        {
            _modelEventHandler = modelEventHandler;
        }

        #endregion

        #region LifeCycle

        private void Awake()
        {
            _modelEventHandler.OnActivatePlayfield += ActivatePlayfieldMenus;
            _modelEventHandler.OnPickupPlayfield += RemovePlayfieldMenus;
        }

        #endregion

        private void ActivatePlayfieldMenus(GameObject playfield)
        {
            _playmatShell = playfield;
            _renderers = _playmatShell.GetComponentsInChildren<MeshRenderer>();

            _menus.SetActive(true);
            SetSliderValues();

            if (_animators == null)
            {
                _animators = GetComponentsInChildren<Animator>();
                _playmatAnimator = _playmatShell.GetComponentInChildren<Animator>();
                return;
            }

            _playmatAnimator.SetTrigger(AnimatorParameters.ActivatePlayfieldTrigger);
        }

        private void RemovePlayfieldMenus()
        {
            _scaleSlider.interactable = false;
            _rotationSlider.interactable = false;
            _menus.SetActive(false);
        }

        private void SetSliderValues()
        {
            var scale = _playmatShell.transform.localScale.x;
            var rotation = _playmatShell.transform.localRotation.y;

            if (scale > 10f)
            {
                _scaleSlider.maxValue = scale;
            }

            _scaleSlider.value = scale;
            _rotationSlider.value = rotation;

            _scaleSlider.interactable = true;
            _rotationSlider.interactable = true;
        }

        public void ScalePlaymat(float scale)
        {
            _playmatShell.transform.localScale = new Vector3(scale, scale, scale);
        }

        public void RotatePlaymat(float rotation) => _playmatShell.transform.rotation = Quaternion.Euler(0, rotation, 0);

        public void FlipPlaymat(bool state)
        {
            if (state)
            {
                _playmatShell.transform.localRotation = Quaternion.Euler(0, 180, 0);
                return;
            }

            _playmatShell.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }


        public void ShowSettingsMenu(bool state)
        {
            foreach (var animator in _animators)
            {
                animator.SetBool(AnimatorParameters.OpenPlayfieldMenuBool, state);
            }
        }

        public void HidePlaymat(bool state)
        {
            foreach (var item in _renderers)
            {
                item.enabled = state;
            }
        }
        
        // TODO: @Subtle can these be removed? They're not used.

        public void DeletePlaymat()
        {
            foreach (var animator in _animators)
            {
                animator.SetTrigger(AnimatorParameters.RemovePlayfieldTrigger);
            }

            _playmatAnimator.SetTrigger(AnimatorParameters.RemovePlayfieldTrigger);
            _toggleView.isOn = false;
        }

        private void DestroyPlaymat()
        {
            _modelEventHandler.DestroyPlayfield();
        }
    }
}