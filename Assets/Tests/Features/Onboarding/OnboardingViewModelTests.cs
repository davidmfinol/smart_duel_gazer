using Code.Core.Logger;
using Code.Core.Navigation;
using Code.Features.Onboarding;
using Moq;
using NUnit.Framework;

namespace Tests.Features.Onboarding
{
    public class OnboardingViewModelTests
    {
        private OnboardingViewModel _viewModel;

        private Mock<INavigationService> _navigationService;
        private Mock<IAppLogger> _logger;

        [SetUp]
        public void SetUp()
        {
            _navigationService = new Mock<INavigationService>();
            _logger = new Mock<IAppLogger>();

            _viewModel = new OnboardingViewModel(
                _navigationService.Object,
                _logger.Object);
        }

        [Test]
        public void When_InitiateLinkPressed_Then_ConnectionSceneShown()
        {
            _viewModel.OnInitiateLinkPressed();
            
            _navigationService.Verify(ns => ns.ShowConnectionScene(), Times.Once);
        }
    }
}