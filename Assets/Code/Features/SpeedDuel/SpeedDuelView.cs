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
using UnityEngine.Serialization;

namespace Code.Features.SpeedDuel
{
    public class SpeedDuelView : MonoBehaviour
    {
        private const string Tag = "SpeedDuelView";

        private const int ExitAnimationsTimeInMs = 700;

        [SerializeField] private Button backButton;
        [SerializeField] private GameObject menu;
        [SerializeField] private Toggle showPlayfieldMenusToggle;
        [SerializeField] private Toggle playmatVisibilityToggle;
        [SerializeField] private Toggle flipPlayfieldToggle;
        [SerializeField] private Button removePlayfieldButton;
        [SerializeField] private Slider rotationSlider;
        [SerializeField] private Slider scaleSlider;
        [SerializeField] private Animator playfieldAnimator;
        [SerializeField] private GameObject settingsMenu;
        
        private SpeedDuelViewModel _speedDuelViewModel;
        private IDelayProvider _delayProvider;
        private IAppLogger _logger;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        #region Construct

        [Inject]
        public void Construct(
            SpeedDuelViewModel speedDuelViewModel,
            IDelayProvider delayProvider,
            IAppLogger appLogger)
        {
            _speedDuelViewModel = speedDuelViewModel;
            _delayProvider = delayProvider;
            _logger = appLogger;

            OnViewModelSet();
        }

        #endregion

        #region Lifecycle

        private void OnDestroy()
        {
            _disposables?.Dispose();
            _speedDuelViewModel?.Dispose();
        }

        #endregion

        private void OnViewModelSet()
        {
            _logger.Log(Tag, "OnViewModelSet()");

            _speedDuelViewModel.Init();

            BindButtons();
        }

        private void BindButtons()
        {
            _logger.Log(Tag, "BindButtons()");

            showPlayfieldMenusToggle.OnValueChangedAsObservable()
                .Subscribe(_speedDuelViewModel.OnTogglePlayfieldMenu);

            // Side Menu Items
            rotationSlider.OnValueChangedAsObservable().Subscribe(value => _speedDuelViewModel.UpdatePlayfieldRotation(value));
            scaleSlider.OnValueChangedAsObservable().Subscribe(value => _speedDuelViewModel.UpdatePlayfieldScale(value));

            // Bottom Menu Items
            playmatVisibilityToggle.OnValueChangedAsObservable()
                .Subscribe(value => _speedDuelViewModel.UpdatePlayfieldVisibility(value));
            flipPlayfieldToggle.OnValueChangedAsObservable().Subscribe(value => _speedDuelViewModel.FlipPlayfield(value));
            removePlayfieldButton.OnClickAsObservable().Subscribe(_ => _speedDuelViewModel.OnRemovePlayfield());

            // Back Button
            backButton.OnClickAsObservable().Subscribe(_ => _speedDuelViewModel.OnBackButtonPressed());

            // VM Streams
            _disposables.Add(_speedDuelViewModel.ActivatePlayfieldMenu
                .Subscribe(ActivatePlayfieldMenu));
            _disposables.Add(_speedDuelViewModel.PlayfieldMenuVisibility
                .Subscribe(UpdatePlayfieldMenuVisibility));
            _disposables.Add(_speedDuelViewModel.RemovePlayfield
                .Subscribe(async state => await RemovePlayfieldMenu(state)));
        }

        private void ActivatePlayfieldMenu(PlayfieldTransformValues playfieldValues)
        {
            _logger.Log(Tag, $"ActivatePlayfieldMenu(playfieldValues: {playfieldValues})");

            menu.SetActive(true);

            if (playfieldValues.Scale > 10f)
            {
                scaleSlider.maxValue = playfieldValues.Scale;
            }

            scaleSlider.value = playfieldValues.Scale;
            rotationSlider.value = playfieldValues.YAxisRotation;
            UpdateSlidersInteractability(true);
        }

        private void UpdatePlayfieldMenuVisibility(bool value)
        {
            _logger.Log(Tag, $"UpdatePlayfieldMenuVisibility(value: {value})");

            playfieldAnimator.SetBool(AnimatorParameters.OpenPlayfieldMenuBool, value);
        }

        private async Task RemovePlayfieldMenu(bool shouldRemove)
        {
            _logger.Log(Tag, $"RemovePlayfieldMenu(shouldRemove: {shouldRemove})");

            if (!shouldRemove) return;

            showPlayfieldMenusToggle.isOn = false;
            playfieldAnimator.SetTrigger(AnimatorParameters.RemovePlayfieldTrigger);

            await _delayProvider.Wait(ExitAnimationsTimeInMs);
            playfieldAnimator.ResetTrigger(AnimatorParameters.RemovePlayfieldTrigger);

            UpdateSlidersInteractability(false);
            menu.SetActive(false);
        }

        private void UpdateSlidersInteractability(bool isInteractable)
        {
            _logger.Log(Tag, $"UpdateSlidersInteractability(isInteractable: {isInteractable})");

            scaleSlider.interactable = isInteractable;
            rotationSlider.interactable = isInteractable;
        }
    }
}