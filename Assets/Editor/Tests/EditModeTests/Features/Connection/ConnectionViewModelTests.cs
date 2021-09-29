using Code.Core.DataManager;
using Code.Core.DataManager.Connections.Entities;
using Code.Core.Dialog;
using Code.Core.Logger;
using Code.Core.Navigation;
using Code.Core.Screen;
using Code.Features.Connection;
using Code.Features.Connection.Helpers;
using Moq;
using NUnit.Framework;

namespace Tests.Features.Connection
{
    public class ConnectionViewModelTests
    {
        private ConnectionViewModel _viewModel;

        private Mock<IDataManager> _dataManager;
        private Mock<IDialogService> _dialogService;
        private Mock<INavigationService> _navigationService;
        private Mock<IAppLogger> _logger;
        private Mock<IScreenService> _screenService;
        private ConnectionFormValidators _validators;

        private const string _validIp = "0.0.0.0";
        private const string _validPort = "8080";

        [SetUp]
        public void SetUp()
        {
            _dataManager = new Mock<IDataManager>();
            _dialogService = new Mock<IDialogService>();
            _navigationService = new Mock<INavigationService>();
            _screenService = new Mock<IScreenService>();
            _logger = new Mock<IAppLogger>();

            _validators = new ConnectionFormValidators();

            _viewModel = new ConnectionViewModel(
                _validators,
                _dataManager.Object,
                _dialogService.Object,
                _navigationService.Object,
                _screenService.Object,
                _logger.Object);
        }

        [Test]
        public void Given_ValidForm_When_EnterLocalDuelRoomButtonPressed_Then_InfoIsSaved()
        {
            var expected = new ConnectionInfo(_validIp, _validPort);

            _viewModel.OnIpAddressChanged(_validIp);
            _viewModel.OnPortChanged(_validPort);
            _viewModel.OnEnterLocalDuelRoomPressed();

            _dataManager.Verify(dm => dm.SaveConnectionInfo(expected), Times.Once);
        }

        [Test]
        public void When_EnterLocalDuelRoomButtonPressed_Then_SaveOnlineUseDuelRoomSavesFalse()
        {
            _viewModel.OnIpAddressChanged(_validIp);
            _viewModel.OnPortChanged(_validPort);
            _viewModel.OnEnterLocalDuelRoomPressed();

            _dataManager.Verify(dm => dm.SaveUseOnlineDuelRoom(false), Times.Once);
        }

        [Test]
        public void Given_ValidForm_When_EnterLocalDuelRoomButtonPressed_Then_DuelRoomSceneIsShown()
        {
            var expected = new ConnectionInfo(_validIp, _validPort);

            _viewModel.OnIpAddressChanged(_validIp);
            _viewModel.OnPortChanged(_validPort);
            _viewModel.OnEnterLocalDuelRoomPressed();

            _navigationService.Verify(ns => ns.ShowDuelRoomScene(), Times.Once);
        }

        [TestCase(null, _validPort)]
        [TestCase(_validIp, null)]
        [TestCase("Invalid", _validPort)]
        [TestCase(_validIp, "Invalid")]
        [Parallelizable()]
        public void Given_InvalidForm_When_EnterLocalDuelRoomButtonPressed_Then_SceneDoesntProgress(
            string ip, string port)
        {
            _viewModel.OnIpAddressChanged(ip);
            _viewModel.OnPortChanged(port);
            _viewModel.OnEnterLocalDuelRoomPressed();

            _navigationService.Verify(ns => ns.ShowDuelRoomScene(), Times.Never);
        }

        [TestCase(null, _validPort, "IP address is required.")]
        [TestCase(_validIp, null, "Port is required.")]
        [TestCase("Invalid", _validPort, "Not a valid IP address.")]
        [TestCase(_validIp, "Invalid", "Not a valid port.")]
        [Parallelizable()]
        public void Given_InvalidForm_When_EnterLocalDuelRoomButtonPressed_Then_ToastMessageShows(
            string ip, string port, string message)
        {
            _viewModel.OnIpAddressChanged(ip);
            _viewModel.OnPortChanged(port);
            _viewModel.OnEnterLocalDuelRoomPressed();

            _dialogService.Verify(ds => ds.ShowToast(message), Times.Once);
        }

        [Test]
        public void When_EnterOnlineDuelRoomButtonPressed_Then_DataMangerSavesTrue()
        {
            _viewModel.OnEnterOnlineDuelRoomPressed();

            _dataManager.Verify(dm => dm.SaveUseOnlineDuelRoom(true), Times.Once);
        }

        [Test]
        public void When_EnterOnlineDuelRoomButtonPressed_Then_DuelRoomSceneShown()
        {
            _viewModel.OnEnterOnlineDuelRoomPressed();

            _navigationService.Verify(ns => ns.ShowDuelRoomScene(), Times.Once);
        }
    }
}