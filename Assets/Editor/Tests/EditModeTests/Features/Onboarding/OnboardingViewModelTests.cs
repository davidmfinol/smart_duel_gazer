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

namespace Editor.Tests.EditModeTests.Features.Onboarding
{
    public class OnboardingViewModelTests
    {
        private OnboardingViewModel _viewModel;

        private Mock<INavigationService> _navigationService;
        private Mock<IFirebaseInitializer> _firebaseInitializer;
        private Mock<IScreenService> _screenService;
        private Mock<IAppLogger> _logger;

        [SetUp]
        public void SetUp()
        {
            _navigationService = new Mock<INavigationService>();
            _firebaseInitializer = new Mock<IFirebaseInitializer>();
            _screenService = new Mock<IScreenService>();
            _logger = new Mock<IAppLogger>();

            _viewModel = new OnboardingViewModel(
                _navigationService.Object,
                _firebaseInitializer.Object,
                _screenService.Object,
                _logger.Object);
        }

        [Test]
        public void When_ViewModelCreated_Then_AppInitializedEmitsFalse()
        {
            var onNext = new List<bool>();
            _viewModel.AppInitialized.Subscribe(value => onNext.Add(value));

            Assert.AreEqual(new List<bool> {false}, onNext);
        }

        [Test]
        public void When_ViewModelInitialized_Then_PortraitOrientationUsed()
        {
            TestUtils.RunAsyncMethodSync(() => _viewModel.Init());
            
            _screenService.Verify(ss => ss.UsePortraitOrientation(), Times.Once);
        }
        
        [Test]
        public void When_ViewModelInitialized_Then_FirebaseInitialized()
        {
            TestUtils.RunAsyncMethodSync(() => _viewModel.Init());
            
            _firebaseInitializer.Verify(fi => fi.Init(), Times.Once);
        }
        
        [Test]
        public void When_ViewModelInitialized_Then_AppInitializedEmitsTrue()
        {
            var onNext = new List<bool>();
            _viewModel.AppInitialized.Subscribe(value => onNext.Add(value));
            
            TestUtils.RunAsyncMethodSync(() => _viewModel.Init());
            
            Assert.AreEqual(new List<bool> {false, true}, onNext);
        }

        [Test]
        public void When_InitiateLinkPressed_Then_ConnectionSceneShown()
        {
            _viewModel.OnInitiateLinkPressed();
            
            _navigationService.Verify(ns => ns.ShowConnectionScene(), Times.Once);
        }
    }
}