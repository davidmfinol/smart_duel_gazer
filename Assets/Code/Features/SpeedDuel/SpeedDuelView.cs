using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;
using Code.Features.SpeedDuel.EventHandlers.Entities;
using Code.Core.Logger;
using Code.UI_Components.Constants;
using System.Threading.Tasks;
using Code.Core.Config.Providers;
using Code.Features.SpeedDuel.Models;

namespace Code.Features.SpeedDuel
{
    public class SpeedDuelView : MonoBehaviour
    {
        private const string Tag = "Speed Duel View";
        
        [SerializeField] private GameObject _menus;
        [SerializeField] private Toggle _togglePlayfieldMenusToggle;
        [SerializeField] private Toggle _hidePlaymatToggle;
        [SerializeField] private Toggle _flipPlayfieldToggle;
        [SerializeField] private Button _removePlayfieldButton;
        [SerializeField] private Slider _rotationSlider;
        [SerializeField] private Slider _scaleSlider;
        [SerializeField] private Animator _animator;

        [SerializeField] private GameObject _settingsMenu;

        [SerializeField] private Button _backButton;

        private SpeedDuelViewModel _speedDuelViewModel;
        private IDelayProvider _delayProvider;
        private IAppLogger _logger;

        CompositeDisposable _disposables = new CompositeDisposable();
        private int ExitAnimationsTimeInMs = 700;

        #region Constructor

        [Inject]
        public void Construct(
            SpeedDuelViewModel speedDuelViewModel,
            IDelayProvider delayProvider,
            IAppLogger appLogger)
        {
            _speedDuelViewModel = speedDuelViewModel;
            _delayProvider = delayProvider;
            _logger = appLogger;

            BindButtons();
        }

        #endregion

        #region Lifecycle

        private void OnDestroy()
        {
            _disposables.Dispose();
            _speedDuelViewModel?.Dispose();
        }

        #endregion

        private void BindButtons()
        {
            _logger.Log(Tag, "BindButtons()");
            
            _togglePlayfieldMenusToggle.OnValueChangedAsObservable()
                .Subscribe(_speedDuelViewModel.OnTogglePlayfieldMenu);

            //Side Menu Items
            _rotationSlider.OnValueChangedAsObservable().Subscribe(_ => _speedDuelViewModel.RotatePlayfield(
                new PlayfieldEventValue<float> { Value = _rotationSlider.value }));
            _scaleSlider.OnValueChangedAsObservable().Subscribe(_ => _speedDuelViewModel.ScalePlayfield(
                new PlayfieldEventValue<float> { Value = _scaleSlider.value }));

            //Bottom Menu Items
            _hidePlaymatToggle.OnValueChangedAsObservable().Subscribe(_ => _speedDuelViewModel.HidePlayfield(
                new PlayfieldEventValue<bool> { Value = _hidePlaymatToggle.isOn }));
            _flipPlayfieldToggle.OnValueChangedAsObservable().Subscribe(_ => _speedDuelViewModel.FlipPlayfield(
                new PlayfieldEventValue<bool> { Value = _flipPlayfieldToggle.isOn }));
            _removePlayfieldButton.OnClickAsObservable().Subscribe(_ => _speedDuelViewModel.OnRemovePlayfield());

            //Back Button
            _backButton.OnClickAsObservable().Subscribe(_ => _speedDuelViewModel.OnBackButtonPressed());

            // VM Streams
            _disposables.Add(_speedDuelViewModel.ActivatePlayfieldMenu
                .Subscribe(playfield => ActivatePlayfieldMenus(playfield)));
            _disposables.Add(_speedDuelViewModel.TogglePlayfieldMenu
                .Subscribe(state => ShowPlayfieldMenu(state)));
            _disposables.Add(_speedDuelViewModel.RemovePlayfield
                .Subscribe(async state => await RemovePlayfieldMenus(state)));            
        }

        #region Functions

        private void ActivatePlayfieldMenus(PlayfieldTransformValues playfieldValues)
        {
            _logger.Log(Tag, "ActivatePlayfieldMenus()");
            
            _menus.SetActive(true);

            if (playfieldValues.Scale > 10f)
            {
                _scaleSlider.maxValue = playfieldValues.Scale;
            }

            _scaleSlider.value = playfieldValues.Scale;
            _rotationSlider.value = playfieldValues.Rotation;
            SetSlidersActive(true);
        }

        private void ShowPlayfieldMenu(bool state)
        {
            _logger.Log(Tag, $"Show Playfield Menus: {state}");
            
            _animator.SetBool(AnimatorParameters.OpenPlayfieldMenuBool, state);
        }

        private async Task RemovePlayfieldMenus(bool state)
        {
            _logger.Log(Tag, "RemovePlayfieldMenus");

            if (!state) return;

            _togglePlayfieldMenusToggle.isOn = false;
            _animator.SetTrigger(AnimatorParameters.RemovePlayfieldTrigger);

            await _delayProvider.Wait(ExitAnimationsTimeInMs);
            _animator.ResetTrigger(AnimatorParameters.RemovePlayfieldTrigger);

            SetSlidersActive(false);
            _menus.SetActive(false);
        }

        private void SetSlidersActive(bool state)
        {
            _scaleSlider.interactable = state;
            _rotationSlider.interactable = state;
        }

        #endregion
    }
}
