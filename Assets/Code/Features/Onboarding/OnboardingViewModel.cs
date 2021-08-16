using Code.Core.Logger;
using Code.Core.Navigation;

namespace Code.Features.Onboarding
{
    public class OnboardingViewModel
    {
        private const string Tag = "OnboardingViewModel";

        private readonly INavigationService _navigationService;
        private readonly IAppLogger _logger;

        public OnboardingViewModel(
            INavigationService navigationService,
            IAppLogger logger)
        {
            _navigationService = navigationService;
            _logger = logger;
        }

        public void OnInitiateLinkPressed()
        {
            _logger.Log(Tag, "OnInitiateLinkPressed()");
            
            _navigationService.ShowConnectionScene();
        }
    }
}
