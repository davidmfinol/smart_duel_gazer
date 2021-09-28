using Code.Core.Logger;
using Code.Features.Onboarding.Models;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.Features.Onboarding
{
    public class OnboardingView : MonoBehaviour
    {
        private const string Tag = "Onboarding View";
        
        [SerializeField] private Button initiateLinkButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private GameObject connectingState;
        [SerializeField] private GameObject noConnectionState;

        private OnboardingViewModel _onboardingViewModel;
        private IAppLogger _logger;

        private CompositeDisposable _disposables = new CompositeDisposable();

        #region Constructor

        [Inject]
        public void Construct(
            OnboardingViewModel onboardingViewModel,
            IAppLogger appLogger)
        {
            _onboardingViewModel = onboardingViewModel;
            _logger = appLogger;

            Init();
        }

        #endregion

        private async void Init()
        {
            _logger.Log(Tag, "Init()");
            
            await _onboardingViewModel.Init();
            
            BindButtons();
        }

        private void BindButtons()
        {
            //Buttons
            initiateLinkButton.OnClickAsObservable()
                .Subscribe(_ => _onboardingViewModel.OnInitiateLinkPressed());
            retryButton.OnClickAsObservable()
                .Subscribe(async _ => await _onboardingViewModel.OnRetryButtonPressed());

            // VM Streams
            _disposables.Add(_onboardingViewModel.HasConnection
                .Subscribe(e => SetInitiateLinkButtonActive(e)));
            _disposables.Add(_onboardingViewModel.UpdateOnboardingState
                .Subscribe(state => UpdateOnboardingState(state)));
        }

        private void UpdateOnboardingState(OnboardingState onboardingState)
        {
            _logger.Log(Tag, $"Update Onboarding State: {onboardingState}");
            
            connectingState.SetActive(OnboardingState.Connecting == onboardingState);
            noConnectionState.SetActive(OnboardingState.NoConnection == onboardingState);
        }

        private void SetInitiateLinkButtonActive(bool isConnected)
        {
            _logger.Log(Tag, $"Set Initiate Button Active: {isConnected}");
            
            initiateLinkButton.interactable = isConnected;
        }
    }
}
