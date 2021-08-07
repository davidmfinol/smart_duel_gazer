using Code.Features.SpeedDuel.EventHandlers;
using Code.UI_Components.Constants;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;
using Code.Features.SpeedDuel.EventHandlers.Entities;
using Code.Core.Config.Providers;

namespace Code.Features.SpeedDuel
{
    public class PlaymatViewLogic : MonoBehaviour
    {
        [SerializeField] private GameObject _menus;        
        [SerializeField] private Toggle _toggleViewToggle;
        [SerializeField] private Toggle _hidePlaymatToggle;
        [SerializeField] private Toggle _flipPlayfieldToggle;
        [SerializeField] private Button _removePlayfieldButton;
        [SerializeField] private Slider _rotationSlider;
        [SerializeField] private Slider _scaleSlider;

        private IDelayProvider _delayProvider;
        private PlayfieldEventHandler _playfieldEventHandler;

        private Animator _animator;
        private int ExitAnimationsTimeInMs = 700;

        #region Constructor

        [Inject]
        public void Construct(
            IDelayProvider delayProvider,
            PlayfieldEventHandler playfieldEventHandler)
        {
            _delayProvider = delayProvider;
            _playfieldEventHandler = playfieldEventHandler;
        }

        #endregion

        #region LifeCycle

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            RegisterClickListeners();

            _playfieldEventHandler.OnActivatePlayfield += ActivatePlayfieldMenus;
        }

        private void OnDestroy()
        {
            _playfieldEventHandler.OnActivatePlayfield -= ActivatePlayfieldMenus;
        }

        #endregion

        private void RegisterClickListeners()
        {
            _toggleViewToggle.OnValueChangedAsObservable().Subscribe(ShowSettingsMenu);

            //Side Menu Items
            _rotationSlider.OnValueChangedAsObservable().Subscribe(RotatePlayfield);
            _scaleSlider.OnValueChangedAsObservable().Subscribe(ScalePlayfield);

            //Bottom Menu Items
            _hidePlaymatToggle.OnValueChangedAsObservable().Subscribe(HidePlayfield);
            _flipPlayfieldToggle.OnValueChangedAsObservable().Subscribe(FlipPlayfield);
            _removePlayfieldButton.OnClickAsObservable().Subscribe(_ => RemovePlayfield());
        }

        private void ActivatePlayfieldMenus()
        {
            _menus.SetActive(true);
            SetSliderValues();
        }

        #region UI Events

        private void ShowSettingsMenu(bool value)
        {
            _animator.SetBool(AnimatorParameters.OpenPlayfieldMenuBool, value);
        }

        private void RotatePlayfield(float rotation)
        {
            _playfieldEventHandler.Action(PlayfieldEvent.Rotate, new PlayfieldEventArgs { floatValue = rotation });
        }

        private void ScalePlayfield(float scale)
        {
            _playfieldEventHandler.Action(PlayfieldEvent.Scale, new PlayfieldEventArgs { floatValue = scale });
        }

        private void HidePlayfield(bool value)
        {
            _playfieldEventHandler.Action(PlayfieldEvent.Hide, new PlayfieldEventArgs { boolValue = value });
        }

        private void FlipPlayfield(bool value)
        {
            _playfieldEventHandler.Action(PlayfieldEvent.Flip, new PlayfieldEventArgs { boolValue = value });
        }

        private void RemovePlayfield()
        {
            _playfieldEventHandler.RemovePlayfield();
            _toggleViewToggle.isOn = false;

            RemovePlayfieldMenus();
        }

        #endregion

        private void SetSliderValues()
        {
            var playfield = FindObjectOfType<PlacementEventHandler>().SpeedDuelField;

            var scale = playfield.transform.localScale.x;
            var rotation = playfield.transform.localRotation.y;

            if (scale > 10f)
            {
                _scaleSlider.maxValue = scale;
            }

            _scaleSlider.value = scale;
            _rotationSlider.value = rotation;

            _scaleSlider.interactable = true;
            _rotationSlider.interactable = true;
        }

        private async void RemovePlayfieldMenus()
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