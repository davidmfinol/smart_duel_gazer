using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.Features.Onboarding
{
    public class OnboardingView : MonoBehaviour
    {
        [SerializeField] private Button initiateLinkButton;

        private OnboardingViewModel _onboardingViewModel;

        [Inject]
        public void Construct(
            OnboardingViewModel onboardingViewModel)
        {
            _onboardingViewModel = onboardingViewModel;

            Init();
        }

        private async void Init()
        {
            await _onboardingViewModel.Init();
            
            BindButtons();
        }

        private void BindButtons()
        {
            initiateLinkButton.OnClickAsObservable().Subscribe(_ => _onboardingViewModel.OnInitiateLinkPressed());
        }
    }
}
