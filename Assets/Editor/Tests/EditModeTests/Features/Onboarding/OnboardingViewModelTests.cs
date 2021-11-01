using System.Collections.Generic;
using Code.Core.Logger;
using Code.Core.Navigation;
using Code.Core.Screen;
using Code.Features.Onboarding;
using Code.Wrappers.WrapperFirebase;
using Editor.Tests.EditModeTests.Utils;
using Moq;
using NUnit.Framework;
using UniRx;
using Code.Wrappers.WrapperNetworkConnection;
using Code.Features.Onboarding.Models;

namespace Editor.Tests.EditModeTests.Features.Onboarding
{
    public class OnboardingViewModelTests
    {
        private OnboardingViewModel _viewModel;

        private Mock<INavigationService> _navigationService;
        private Mock<INetworkConnectionProvider> _networkConnectionProvider;
        private Mock<IFirebaseInitializer> _firebaseInitializer;
        private Mock<IScreenService> _screenService;
        private Mock<IAppLogger> _logger;

        [SetUp]
        public void SetUp()
        {
            _navigationService = new Mock<INavigationService>();
            _networkConnectionProvider = new Mock<INetworkConnectionProvider>();
            _firebaseInitializer = new Mock<IFirebaseInitializer>();
            _screenService = new Mock<IScreenService>();
            _logger = new Mock<IAppLogger>();

            _viewModel = new OnboardingViewModel(
                _navigationService.Object,
                _networkConnectionProvider.Object,
                _firebaseInitializer.Object,
                _screenService.Object,
                _logger.Object);
        }

        [Test]
        public void When_ViewModelCreated_Then_StateEmitsConnecting()
        {
            var expected = new List<OnboardingState> { OnboardingState.Connecting };
            
            var onNext = new List<OnboardingState>();
            _viewModel.State.Subscribe(value => onNext.Add(value));

            Assert.AreEqual(expected, onNext);
        }

        [Test]
        public void When_ViewModelInitialized_Then_PortraitOrientationUsed()
        {
            TestUtils.RunAsyncMethodSync(() => _viewModel.Init());

            _screenService.Verify(ss => ss.UsePortraitOrientation(), Times.Once);
        }

        [Test]
        public void When_ViewModelInitialized_Then_NetworkConnectionChecked()
        {
            TestUtils.RunAsyncMethodSync(() => _viewModel.Init());

            _networkConnectionProvider.Verify(ncp => ncp.IsConnected(), Times.Once);
        }

        [Test]
        public void Given_NoNetworkConnection_When_ViewModelInitialized_Then_NoConnectionStateEmitted()
        {
            var expected = new List<OnboardingState> { OnboardingState.Connecting, OnboardingState.NoConnection };
            
            var onNext = new List<OnboardingState>();
            _viewModel.State.Subscribe(value => onNext.Add(value));
            _networkConnectionProvider.Setup(ncp => ncp.IsConnected()).Returns(false);

            TestUtils.RunAsyncMethodSync(() => _viewModel.Init());

            Assert.AreEqual(expected, onNext);
        }

        [Test]
        public void Given_NetworkConnection_When_ViewModelInitialized_Then_ConnectedStateEmitted()
        {
            var expected = new List<OnboardingState> { OnboardingState.Connecting, OnboardingState.Connected };
            
            var onNext = new List<OnboardingState>();
            _viewModel.State.Subscribe(value => onNext.Add(value));
            _networkConnectionProvider.Setup(ncp => ncp.IsConnected()).Returns(true);

            TestUtils.RunAsyncMethodSync(() => _viewModel.Init());

            Assert.AreEqual(expected, onNext);
        }
        
        [Test]
        public void When_ViewModelInitialized_Then_FirebaseInitialized()
        {
            TestUtils.RunAsyncMethodSync(() => _viewModel.Init());

            _firebaseInitializer.Verify(fi => fi.Init(), Times.Once);
        }
        
        [Test]
        public void Given_NoNetworkConnection_When_InitiateLinkPressed_Then_NoConnectionStateEmitted()
        {
            var expected = new List<OnboardingState> { OnboardingState.Connecting, OnboardingState.NoConnection };
            
            var onNext = new List<OnboardingState>();
            _viewModel.State.Subscribe(value => onNext.Add(value));
            _networkConnectionProvider.Setup(ncp => ncp.IsConnected()).Returns(false);
            
            _viewModel.OnInitiateLinkPressed();

            Assert.AreEqual(expected, onNext);
        }
        
        [Test]
        public void Given_NetworkConnection_When_InitiateLinkPressed_Then_ConnectionSceneShown()
        {
            _networkConnectionProvider.Setup(ncp => ncp.IsConnected()).Returns(true);
            
            _viewModel.OnInitiateLinkPressed();
            
            _navigationService.Verify(ns => ns.ShowConnectionScene(), Times.Once);
        }
        
        [Test]
        public void Given_NoNetworkConnection_When_RetryButtonPressed_Then_NoConnectionStateEmitted()
        {
            var expected = new List<OnboardingState> { OnboardingState.Connecting, OnboardingState.Connecting, OnboardingState.NoConnection };
            
            var onNext = new List<OnboardingState>();
            _viewModel.State.Subscribe(value => onNext.Add(value));
            _networkConnectionProvider.Setup(ncp => ncp.IsConnected()).Returns(false);

            _viewModel.OnRetryButtonPressed();

            Assert.AreEqual(expected, onNext);
        }

        [Test]
        public void Given_NetworkConnection_When_RetryButtonPressed_Then_ConnectedStateEmitted()
        {
            var expected = new List<OnboardingState> { OnboardingState.Connecting, OnboardingState.Connecting, OnboardingState.Connected };
            
            var onNext = new List<OnboardingState>();
            _viewModel.State.Subscribe(value => onNext.Add(value));
            _networkConnectionProvider.Setup(ncp => ncp.IsConnected()).Returns(true);

            _viewModel.OnRetryButtonPressed();

            Assert.AreEqual(expected, onNext);
        }
    }
}
