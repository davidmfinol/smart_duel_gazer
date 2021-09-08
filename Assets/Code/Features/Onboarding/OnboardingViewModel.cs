using System;
using System.Threading.Tasks;
using Code.Core.Logger;
using Code.Core.Navigation;
using Code.Core.Screen;
using Code.Wrappers.WrapperFirebase;
using UniRx;

namespace Code.Features.Onboarding
{
    public class OnboardingViewModel
    {
        private const string Tag = "OnboardingViewModel";

        private readonly INavigationService _navigationService;
        private readonly IFirebaseInitializer _firebaseInitializer;
        private readonly IScreenService _screenService;
        private readonly IAppLogger _logger;
        
        private readonly BehaviorSubject<bool> _appInitialized = new BehaviorSubject<bool>(false);
        public IObservable<bool> AppInitialized => _appInitialized;

        public OnboardingViewModel(
            INavigationService navigationService,
            IFirebaseInitializer firebaseInitializer,
            IScreenService screenService,
            IAppLogger logger)
        {
            _navigationService = navigationService;
            _firebaseInitializer = firebaseInitializer;
            _screenService = screenService;
            _logger = logger;
        }

        public async Task Init()
        {
            _logger.Log(Tag, "Init()");

            _screenService.UsePortraitOrientation();
            
            await _firebaseInitializer.Init();
            
            _appInitialized.OnNext(true);
        }

        public void OnInitiateLinkPressed()
        {
            _logger.Log(Tag, "OnInitiateLinkPressed()");
            
            _navigationService.ShowConnectionScene();
        }
    }
}
