using Zenject;
using UnityEngine;
using UnityEngine.UI;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler;
using AssemblyCSharp.Assets.Code.UIComponents.Constants;

namespace AssemblyCSharp.Assets.Code.Features.SpeedDuel
{
    public class PlaymatViewLogic : MonoBehaviour
    {
        [SerializeField]
        private GameObject _menus;
        [SerializeField]
        private GameObject _bottomMenu;
        [SerializeField]
        private Slider _scaleSlider;
        [SerializeField]
        private Toggle _toggleView;

        private ModelEventHandler _modelEventHandler;

        private GameObject _playmatShell;
        private MeshRenderer[] _renderers;
        private Animator[] _animators;

        #region Constructor

        [Inject]
        public void Construct(ModelEventHandler modelEventHandler)
        {
            _modelEventHandler = modelEventHandler;
        }

        #endregion

        private void Awake()
        {
            _modelEventHandler.OnActivatePlayfield += ActivatePlayfieldMenus;
            _modelEventHandler.OnRemovePlayfield += RemovePlayfieldMenus;
        }

        private void ActivatePlayfieldMenus(GameObject playfield)
        {
            _playmatShell = playfield;
            _renderers = _playmatShell.GetComponentsInChildren<MeshRenderer>();

            _menus.SetActive(true);

            if (_animators == null)
            {
                _animators = GetComponentsInChildren<Animator>();
            }
        }

        private void RemovePlayfieldMenus()
        {
            _menus.SetActive(false);
        }

        public void SetScaleStartPosition(float startPosition) => _scaleSlider.value = startPosition;

        public void ScalePlaymat(float scale)
        {
            var mappedScale = MapScaleValueToSliderValue(scale, _scaleSlider.minValue, _scaleSlider.maxValue, 0.1f, 5f);
            _playmatShell.transform.localScale = new Vector3(mappedScale, mappedScale, mappedScale);
        }

        private float MapScaleValueToSliderValue(float value,
                                                 float originalMin,
                                                 float originalMax,
                                                 float newMin,
                                                 float newMax)
        {
            return (value - originalMin) / (originalMax - originalMin) * (newMax - newMin) + newMin;
        }

        public void RotatePlaymat(float rotation) => _playmatShell.transform.rotation = Quaternion.Euler(0, rotation, 0);

        public void FlipPlaymat(bool state)
        {
            if (state)
            {
                _playmatShell.transform.rotation = Quaternion.Euler(0, 180, 0);
                return;
            }

            _playmatShell.transform.rotation = Quaternion.Euler(0, 0, 0);
        }


        public void ShowSettingsMenu(bool state)
        {
            foreach (Animator animator in _animators)
            {
                animator.SetBool(AnimatorParameters.OpenPlayfieldMenuBool, state);
            }
        }

        public void HidePlaymat(bool state)
        {
            foreach (MeshRenderer item in _renderers)
            {
                item.enabled = state;
            }
        }

        public void DeletePlaymat()
        {
            foreach (Animator animator in _animators)
            {
                animator.SetTrigger(AnimatorParameters.RemovePlayfieldTrigger);
            }
            _playmatShell.GetComponentInChildren<Animator>().SetTrigger(AnimatorParameters.RemovePlayfieldTrigger);
            _toggleView.isOn = false;
        }

        private void DestroyPlaymat()
        {
            _modelEventHandler.DestroyPlayfield();
        }
    }
}
