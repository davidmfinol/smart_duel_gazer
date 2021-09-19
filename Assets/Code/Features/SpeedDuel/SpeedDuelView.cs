using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;
using Code.Features.SpeedDuel.EventHandlers.Entities;

namespace Code.Features.SpeedDuel
{
    public class SpeedDuelView : MonoBehaviour
    {
        [SerializeField] private GameObject _menus;
        [SerializeField] private Toggle _toggleViewToggle;
        [SerializeField] private Toggle _hidePlaymatToggle;
        [SerializeField] private Toggle _flipPlayfieldToggle;
        [SerializeField] private Button _removePlayfieldButton;
        [SerializeField] private Slider _rotationSlider;
        [SerializeField] private Slider _scaleSlider;

        [SerializeField] private GameObject _settingsMenu;

        [SerializeField] private Button _backButton;

        private SpeedDuelViewModel _speedDuelViewModel;
        private PlayfieldMenuComponentsManager _playfieldMenuComponentsManager;

        [Inject]
        public void Construct(
            SpeedDuelViewModel speedDuelViewModel)
        {
            _speedDuelViewModel = speedDuelViewModel;

            Init();
        }

        private void Init()
        {
            _playfieldMenuComponentsManager = GetComponentInChildren<PlayfieldMenuComponentsManager>();
            if (_playfieldMenuComponentsManager == null) return;

            _playfieldMenuComponentsManager.InitMenus(_menus, _toggleViewToggle, _rotationSlider, _scaleSlider);
            BindButtons();
        }

        private void BindButtons()
        {
            _toggleViewToggle.OnValueChangedAsObservable().Subscribe(_playfieldMenuComponentsManager.ShowSettingsMenu);

            //Side Menu Items
            _rotationSlider.OnValueChangedAsObservable().Subscribe(_ => _speedDuelViewModel.RotatePlayfield(
                new PlayfieldEventArgs { FloatValue = _rotationSlider.value }));
            _scaleSlider.OnValueChangedAsObservable().Subscribe(_ => _speedDuelViewModel.ScalePlayfield(
                new PlayfieldEventArgs { FloatValue = _scaleSlider.value }));

            //Bottom Menu Items
            _hidePlaymatToggle.OnValueChangedAsObservable().Subscribe(_ => _speedDuelViewModel.HidePlayfield(
                new PlayfieldEventArgs { BoolValue = _hidePlaymatToggle.isOn }));
            _flipPlayfieldToggle.OnValueChangedAsObservable().Subscribe(_ => _speedDuelViewModel.FlipPlayfield(
                new PlayfieldEventArgs { BoolValue = _flipPlayfieldToggle.isOn }));
            _removePlayfieldButton.OnClickAsObservable().Subscribe(_ => _playfieldMenuComponentsManager.RemovePlayfield());

            //Back Button
            _backButton.OnClickAsObservable().Subscribe(_ => _speedDuelViewModel.OnBackButtonPressed());
        }
    }
}
