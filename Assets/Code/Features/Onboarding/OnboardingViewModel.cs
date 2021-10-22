using System;
using System.Threading.Tasks;
using Code.Core.Logger;
using Code.Core.Navigation;
using Code.Core.Screen;
using Code.Features.Onboarding.Models;
using Code.Wrappers.WrapperNetworkConnection;
using Code.Wrappers.WrapperFirebase;
using UniRx;

namespace Code.Features.Onboarding
{
    public class OnboardingViewModel
    {
        private const string Tag = "OnboardingViewModel";

        private readonly INavigationService _navigationService;
        private readonly INetworkConnectionProvider _networkConnectionProvider;
        private readonly IFirebaseInitializer _firebaseInitializer;
        private readonly IScreenService _screenService;
        private readonly IAppLogger _logger;

        #region Properties

        private readonly BehaviorSubject<OnboardingState> _onboardingState =
            new BehaviorSubject<OnboardingState>(OnboardingState.Connecting);

        public IObservable<OnboardingState> State => _onboardingState;

        #endregion

        #region Constructor

        public OnboardingViewModel(
            INavigationService navigationService,
            INetworkConnectionProvider networkConnectionProvider,
            IFirebaseInitializer firebaseInitializer,
            IScreenService screenService,
            IAppLogger logger)
        {
            _navigationService = navigationService;
            _networkConnectionProvider = networkConnectionProvider;
            _firebaseInitializer = firebaseInitializer;
            _screenService = screenService;
            _logger = logger;
        }

        #endregion

        public async Task Init()
        {
            _logger.Log(Tag, "Init()");

            _screenService.UsePortraitOrientation();
            CheckNetworkConnection();

            await _firebaseInitializer.Init();
        }

        private void CheckNetworkConnection()
        {
            _logger.Log(Tag, "CheckNetworkConnection()");

            var isConnected = _networkConnectionProvider.IsConnected();
            var state = isConnected ? OnboardingState.Connected : OnboardingState.NoConnection;
            _onboardingState.OnNext(state);
        }

        public void OnInitiateLinkPressed()
        {
            _logger.Log(Tag, "OnInitiateLinkPressed()");

            if (!_networkConnectionProvider.IsConnected())
            {
                _onboardingState.OnNext(OnboardingState.NoConnection);
                return;
            }

            _navigationService.ShowConnectionScene();
        }

        public void OnRetryButtonPressed()
        {
            _logger.Log(Tag, "OnRetryButtonPressed()");

            _onboardingState.OnNext(OnboardingState.Connecting);

            CheckNetworkConnection();
        }

        public void Dispose()
        {
            _logger.Log(Tag, "Dispose()");

            _onboardingState.Dispose();
        }
    }
}