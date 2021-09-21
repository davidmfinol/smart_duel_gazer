using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;
using Code.Features.SpeedDuel.EventHandlers.Entities;
using Code.Features.SpeedDuel.EventHandlers;
using Code.Core.Logger;
using Code.UI_Components.Constants;
using System.Threading.Tasks;
using Code.Core.Config.Providers;

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
        private PlayfieldMenuComponentsManager _playfieldMenuComponentsManager;
        private PlacementEventHandler _placementEventHandler;
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

            Init();
        }

        #endregion

        #region Lifecycle

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        #endregion

        private void Init()
        {
            _playfieldMenuComponentsManager = GetComponentInChildren<PlayfieldMenuComponentsManager>();
            _placementEventHandler = FindObjectOfType<PlacementEventHandler>();
            if (_playfieldMenuComponentsManager == null || _placementEventHandler == null) return;

            _playfieldMenuComponentsManager.InitMenus(_rotationSlider, _scaleSlider);
            BindButtons();
        }

        private void BindButtons()
        {
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
            _disposables.Add(_speedDuelViewModel.TogglePlayfieldMenu
                .Subscribe(state => ShowPlayfieldMenu(state)));
            _disposables.Add(_speedDuelViewModel.RemovePlayfield
                .Subscribe(async state => await RemovePlayfieldMenus(state)));

            // Placement Event Handler Streams
            _disposables.Add(_placementEventHandler.ActivatePlayfield
                .Subscribe(state => ActivatePlayfieldMenus(state)));
        }

        #region Functions

        private void ActivatePlayfieldMenus(bool state)
        {
            _logger.Log(Tag, $"Activate Playfield Menus: {state}");
            
            if (!state) return;
            
            _menus.SetActive(true);
            var playfieldValues = _playfieldMenuComponentsManager.SetSliderValues();

            _scaleSlider.value = playfieldValues.Scale;
            _rotationSlider.value = playfieldValues.Rotation;
            SetSlidersActive(true);
        }

        private void ShowPlayfieldMenu(bool state)
        {
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
