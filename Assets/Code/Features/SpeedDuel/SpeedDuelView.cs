using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;
using Code.Core.Logger;

namespace Code.Features.SpeedDuel
{
    public class SpeedDuelView : MonoBehaviour
    {
        private const string Tag = "SpeedDuelView";

        [SerializeField] private Toggle showSettingsMenuToggle;
        [SerializeField] private Slider transparencySlider;
        [SerializeField] private Slider scaleSlider;
        [SerializeField] private Button backButton;
        [SerializeField] private Button removePlayfieldButton;
        [SerializeField] private GameObject settingsMenu;

        private SpeedDuelViewModel _speedDuelViewModel;
        private IAppLogger _logger;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        #region Construct

        [Inject]
        public void Construct(
            SpeedDuelViewModel speedDuelViewModel,
            IAppLogger appLogger)
        {
            _speedDuelViewModel = speedDuelViewModel;
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

            // Toggles
            showSettingsMenuToggle.OnValueChangedAsObservable()
                .Subscribe(_speedDuelViewModel.OnToggleSettingsMenu);

            // Sliders
            transparencySlider.OnValueChangedAsObservable().Subscribe(value => _speedDuelViewModel.UpdatePlayfieldTransparency(value));
            scaleSlider.OnValueChangedAsObservable().Subscribe(value => _speedDuelViewModel.UpdatePlayfieldScale(value));

            // Buttons
            removePlayfieldButton.OnClickAsObservable().Subscribe(_ => _speedDuelViewModel.OnRemovePlayfield());
            backButton.OnClickAsObservable().Subscribe(_ => _speedDuelViewModel.OnBackButtonPressed());

            // VM Streams
            _disposables.Add(_speedDuelViewModel.ShowSettingsMenu
                .Subscribe(UpdateSettingsMenuVisibility));
            _disposables.Add(_speedDuelViewModel.ActivatePlayfieldUIElements
                .Subscribe(ActivatePlayfieldUIElements));
            _disposables.Add(_speedDuelViewModel.RemovePlayfield
                .Subscribe(SetElementsInteractableState));
        }

        private void UpdateSettingsMenuVisibility(bool value)
        {
            _logger.Log(Tag, $"UpdateSettingsMenuVisibility(value: {value})");

            settingsMenu.SetActive(value);
        }

        private void ActivatePlayfieldUIElements(float playfieldScale)
        {
            _logger.Log(Tag, $"ActivatePlayfieldMenu(playfieldScale: {playfieldScale})");

            if (playfieldScale > 10f)
            {
                scaleSlider.maxValue = playfieldScale;
            }

            scaleSlider.value = playfieldScale;
            SetElementsInteractableState(true);
        }

        private void SetElementsInteractableState(bool isInteractable)
        {
            _logger.Log(Tag, $"SetElementsInteractableState(isInteractable: {isInteractable})");

            scaleSlider.interactable = isInteractable;
            transparencySlider.interactable = isInteractable;
            removePlayfieldButton.interactable = isInteractable;
        }
    }
}