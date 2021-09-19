using Code.Core.Config.Providers;
using Code.Features.SpeedDuel.EventHandlers;
using Code.Features.SpeedDuel.PrefabManager.Prefabs.Playfield.Scripts;
using Code.UI_Components.Constants;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.Features.SpeedDuel
{
    //TODO: Make this class redundant and move logic to SpeedDuelViewModel
    public class PlayfieldMenuComponentsManager : MonoBehaviour
    {
        private int ExitAnimationsTimeInMs = 700;

        [SerializeField] private Animator _animator;

        private GameObject _playfield;
        private GameObject _menus;
        private Toggle _toggleViewToggle;
        private Slider _rotationSlider;
        private Slider _scaleSlider;

        private IDelayProvider _delayProvider;
        private IPlayfieldEventHandler _playfieldEventHandler;

        #region Constructor

        [Inject]
        public void Construct(
            IDelayProvider delayProvider,
            IPlayfieldEventHandler playfieldEventHandler)
        {
            _delayProvider = delayProvider;
            _playfieldEventHandler = playfieldEventHandler;
        }

        #endregion

        #region Lifecycle

        private void Awake()
        {
            //_animator = GetComponent<Animator>();
            _playfieldEventHandler.OnActivatePlayfield += ActivatePlayfieldMenus;
        }

        private void OnDestroy()
        {
            _playfieldEventHandler.OnActivatePlayfield -= ActivatePlayfieldMenus;
        }

        #endregion

        public void InitMenus(
            GameObject menus,
            Toggle toggleViewToggle,
            Slider rotationSlider,
            Slider scaleSlider)
        {
            _menus = menus;
            _toggleViewToggle = toggleViewToggle;
            _rotationSlider = rotationSlider;
            _scaleSlider = scaleSlider;
        }

        public void ActivatePlayfieldMenus()
        {
            _menus.SetActive(true);
            SetSliderValues();
        }

        public void RemovePlayfield()
        {
            _playfieldEventHandler.RemovePlayfield();
            _toggleViewToggle.isOn = false;

            RemovePlayfieldMenus();
        }

        public void ShowSettingsMenu(bool value)
        {
            _animator.SetBool(AnimatorParameters.OpenPlayfieldMenuBool, value);
        }

        public void SetSliderValues()
        {
            _playfield = FindObjectOfType<PlayfieldComponentsManager>().gameObject;
            if (_playfield == null) return;

            var scale = _playfield.transform.localScale.x;
            var rotation = _playfield.transform.localRotation.y;

            if (scale > 10f)
            {
                _scaleSlider.maxValue = scale;
            }

            _scaleSlider.value = scale;
            _rotationSlider.value = rotation;

            _scaleSlider.interactable = true;
            _rotationSlider.interactable = true;
        }

        public async void RemovePlayfieldMenus()
        {
            _animator.SetTrigger(AnimatorParameters.RemovePlayfieldTrigger);

            await _delayProvider.Wait(ExitAnimationsTimeInMs);
            _animator.ResetTrigger(AnimatorParameters.RemovePlayfieldTrigger);

            _scaleSlider.interactable = false;
            _rotationSlider.interactable = false;
            _menus.SetActive(false);
        }
    }
}