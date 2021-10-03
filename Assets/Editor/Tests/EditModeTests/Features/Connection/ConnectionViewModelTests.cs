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
using System.Collections.Generic;
using UniRx;

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

        #region Initialization

        [Test]
        public void When_ViewModelCreated_Then_ToggleSettingsMenuEmitsFalse()
        {
            var onNext = new List<bool>();
            _viewModel.ToggleSettingsMenu.Subscribe(value => onNext.Add(value));

            Assert.AreEqual(new List<bool> { false }, onNext);
        }

        [Test]
        public void When_ViewModelCreated_Then_ToggleDeveloperModeEmitsFalse()
        {
            var onNext = new List<bool>();
            _viewModel.ToggleDeveloperMode.Subscribe(value => onNext.Add(value));

            Assert.AreEqual(new List<bool> { false }, onNext);
        }

        [Test]
        public void When_ViewModelCreated_Then_ToggleLocalConnectionMenuEmitsFalse()
        {
            var onNext = new List<bool>();
            _viewModel.ToggleLocalConnectionMenu.Subscribe(value => onNext.Add(value));

            Assert.AreEqual(new List<bool> { false }, onNext);
        }

        [Test]
        public void When_ViewModelInitialized_PortraitOrientationUsed()
        {
            _viewModel.Init();

            _screenService.Verify(ss => ss.UsePortraitOrientation(), Times.Once);
        }

        [Test]
        public void When_ViewModelInitialized_Then_DataManagerGetsSavedDeveloperModeSetting()
        {
            _viewModel.Init();

            _dataManager.Verify(dm => dm.IsDeveloperModeEnabled(), Times.Once);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void When_ViewModelInitialized_Then_ToggleDeveloperModeEmitsStateFromDataManager(bool state)
        {
            var onNext = new List<bool>();
            _viewModel.ToggleDeveloperMode.Subscribe(value => onNext.Add(value));
            _dataManager.Setup(dm => dm.IsDeveloperModeEnabled()).Returns(state);

            _viewModel.Init();

            Assert.AreEqual(new List<bool> { false, state }, onNext);
        }

        [Test]
        public void When_ViewModelInitialized_Then_DataManagerReturnsForcedLocalConnectionInfo()
        {
            _viewModel.Init();

            _dataManager.Verify(dm => dm.GetConnectionInfo(true), Times.Once);
        }

        [Test]
        public void Given_NullConnectionInfo_When_ViewModelInitialized_Then_ipAddressEmitsNoEvents()
        {
            var onNext = new List<string>();
            _viewModel.IpAddress.Subscribe(value => onNext.Add(value));
            ConnectionInfo nullInfo = null;
            _dataManager.Setup(dm => dm.GetConnectionInfo(true)).Returns(nullInfo);

            _viewModel.Init();

            Assert.AreEqual(new List<string> { null }, onNext);
        }

        [Test]
        public void Given_NullConnectionInfo_When_ViewModelInitialized_Then_PortEmitsNoEvents()
        {
            var onNext = new List<string>();
            _viewModel.Port.Subscribe(value => onNext.Add(value));
            ConnectionInfo nullInfo = null;
            _dataManager.Setup(dm => dm.GetConnectionInfo(true)).Returns(nullInfo);

            _viewModel.Init();

            Assert.AreEqual(new List<string> { null }, onNext);
        }

        [Test]
        public void Given_ConnectionInfo_When_ViewModelInitialized_Then_ipAddressEmitsipAddressValue()
        {
            var onNext = new List<string>();
            _viewModel.IpAddress.Subscribe(value => onNext.Add(value));
            _dataManager.Setup(dm => dm.GetConnectionInfo(true)).Returns(new ConnectionInfo(_validIp, _validPort));

            _viewModel.Init();

            Assert.AreEqual(new List<string> { null, _validIp }, onNext);
        }

        [Test]
        public void Given_ConnectionInfo_When_ViewModelInitialized_Then_PortEmitsPortValue()
        {
            var onNext = new List<string>();
            _viewModel.Port.Subscribe(value => onNext.Add(value));
            _dataManager.Setup(dm => dm.GetConnectionInfo(true)).Returns(new ConnectionInfo(_validIp, _validPort));

            _viewModel.Init();

            Assert.AreEqual(new List<string> { null, _validPort }, onNext);
        }

        #endregion

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

        [TestCase(true)]
        [TestCase(false)]
        public void When_ToggleSettingsMenuPressed_Then_OnToggleSettingsMenuEmitsGivenValue(bool state)
        {
            var onNext = new List<bool>();
            _viewModel.ToggleSettingsMenu.Subscribe(value => onNext.Add(value));

            _viewModel.OnSettingsMenuToggled(state);

            Assert.AreEqual(new List<bool> { false, state }, onNext);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void When_DeveloperModeToggled_Then_DataManagerSavesState(bool state)
        {
            _viewModel.OnDeveloperModeToggled(state);

            _dataManager.Verify(dm => dm.SaveDeveloperModeEnabled(state), Times.Once);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void When_DeveloperModeToggled_Then_OnToggleLocalConnectionMenuEmitsGivenState(bool state)
        {
            var onNext = new List<bool>();
            _viewModel.ToggleLocalConnectionMenu.Subscribe(value => onNext.Add(value));

            _viewModel.OnDeveloperModeToggled(state);

            Assert.AreEqual(new List<bool> { false, state }, onNext);
        }
    }
}