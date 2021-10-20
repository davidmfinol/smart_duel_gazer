using Code.Features.Onboarding.Models;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.Features.Onboarding
{
    public class OnboardingView : MonoBehaviour
    {
        [SerializeField] private Button initiateLinkButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private GameObject connectingState;
        [SerializeField] private GameObject noConnectionState;

        private OnboardingViewModel _onboardingViewModel;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        #region Lifecycle
        
        [Inject]
        public void Construct(
            OnboardingViewModel onboardingViewModel)
        {
            _onboardingViewModel = onboardingViewModel;

            OnViewModelSet();
        }
        
        private void OnDestroy()
        {
            _disposables?.Dispose();
            _onboardingViewModel?.Dispose();
        }

        #endregion
        
        private async void OnViewModelSet()
        {
            await _onboardingViewModel.Init();

            BindButtons();
        }

        private void BindButtons()
        {
            //Buttons
            initiateLinkButton.OnClickAsObservable()
                .Subscribe(_ => _onboardingViewModel.OnInitiateLinkPressed());
            retryButton.OnClickAsObservable()
                .Subscribe(_ => _onboardingViewModel.OnRetryButtonPressed());

            // VM Streams
            _disposables.Add(_onboardingViewModel.UpdateOnboardingState
                .Subscribe(UpdateOnboardingState));
        }

        private void UpdateOnboardingState(OnboardingState onboardingState)
        {
            connectingState.SetActive(onboardingState == OnboardingState.Connecting);
            noConnectionState.SetActive(onboardingState == OnboardingState.NoConnection);
            initiateLinkButton.interactable = onboardingState == OnboardingState.Connected;
        }
    }
}