using System.Collections.Generic;
using Code.Core.DataManager;
using Code.Core.DataManager.Connections.Entities;
using Code.Core.Dialog;
using Code.Core.Localization;
using Code.Core.Localization.Entities;
using Code.Core.Logger;
using Code.Core.Navigation;
using Code.Core.Screen;
using Code.Features.Connection;
using Code.Features.Connection.Helpers;
using Moq;
using NUnit.Framework;
using UniRx;

namespace Editor.Tests.EditModeTests.Features.Connection
{
    public class ConnectionViewModelTests
    {
        private ConnectionViewModel _viewModel;

        private ConnectionFormValidators _validators;
        private Mock<IDataManager> _dataManager;
        private Mock<IDialogService> _dialogService;
        private Mock<INavigationService> _navigationService;
        private Mock<IScreenService> _screenService;
        private Mock<IStringProvider> _stringProvider;
        private Mock<IAppLogger> _logger;

        private const string ValidIp = "0.0.0.0";
        private const string ValidPort = "8080";

        private static readonly ConnectionInfo ConnectionInfo = new ConnectionInfo(ValidIp, ValidPort);

        [SetUp]
        public void SetUp()
        {
            _validators = new ConnectionFormValidators();
            _dataManager = new Mock<IDataManager>();
            _dialogService = new Mock<IDialogService>();
            _navigationService = new Mock<INavigationService>();
            _screenService = new Mock<IScreenService>();
            _stringProvider = new Mock<IStringProvider>();
            _logger = new Mock<IAppLogger>();

            _stringProvider.Setup(sp => sp.GetString(
                It.IsAny<string>(), It.IsAny<object[]>())).Returns<string, object[]>((key, args) => key);

            _viewModel = new ConnectionViewModel(
                _validators,
                _dataManager.Object,
                _dialogService.Object,
                _navigationService.Object,
                _screenService.Object,
                _stringProvider.Object,
                _logger.Object);
        }

        [Test]
        public void When_ViewModelCreated_Then_ShowSettingsMenuEmitsFalse()
        {
            var onNext = new List<bool>();
            _viewModel.ShowSettingsMenu.Subscribe(value => onNext.Add(value));

            Assert.AreEqual(new List<bool> { false }, onNext);
        }

        [Test]
        public void When_ViewModelCreated_Then_IsDeveloperModeEnabledEmitsFalse()
        {
            var onNext = new List<bool>();
            _viewModel.IsDeveloperModeEnabled.Subscribe(value => onNext.Add(value));

            Assert.AreEqual(new List<bool> { false }, onNext);
        }

        [Test]
        public void When_ViewModelCreated_Then_ShowLocalConnectionMenuEmitsFalse()
        {
            var onNext = new List<bool>();
            _viewModel.ShowLocalConnectionMenu.Subscribe(value => onNext.Add(value));

            Assert.AreEqual(new List<bool> { false }, onNext);
        }

        [Test]
        public void When_ViewModelInitialized_Then_PortraitOrientationUsed()
        {
            _viewModel.Init();

            _screenService.Verify(ss => ss.UsePortraitOrientation(), Times.Once);
        }

        [Test]
        public void When_ViewModelInitialized_Then_DeveloperModeEnabledSettingFetched()
        {
            _viewModel.Init();

            _dataManager.Verify(dm => dm.IsDeveloperModeEnabled(), Times.Once);
        }

        [Test]
        public void When_ViewModelInitialized_Then_IsDeveloperModeEnabledSettingValueEmitted()
        {
            const bool developerModeEnabled = true;
            
            var onNext = new List<bool>();
            _viewModel.IsDeveloperModeEnabled.Subscribe(value => onNext.Add(value));
            _dataManager.Setup(dm => dm.IsDeveloperModeEnabled()).Returns(developerModeEnabled);

            _viewModel.Init();

            Assert.AreEqual(new List<bool> { false, developerModeEnabled }, onNext);
        }

        [Test]
        public void When_ViewModelInitialized_Then_DataManagerReturnsForcedLocalConnectionInfo()
        {
            _viewModel.Init();

            _dataManager.Verify(dm => dm.GetConnectionInfo(true), Times.Once);
        }

        [Test]
        public void When_ViewModelInitialized_Then_ConnectionInfoFetched()
        {
            _viewModel.Init();

            _dataManager.Verify(dm => dm.GetConnectionInfo(true), Times.Once);
        }

        [Test]
        public void Given_ConnectionInfoNotNull_When_ViewModelInitialized_Then_ConnectionInfoEmitted()
        {
            _dataManager.Setup(dm => dm.GetConnectionInfo(true)).Returns(ConnectionInfo);

            var ipAddressOnNext = new List<string>();
            var portOnNext = new List<string>();
            _viewModel.IpAddress.Subscribe(value => ipAddressOnNext.Add(value));
            _viewModel.Port.Subscribe(value => portOnNext.Add(value));

            _viewModel.Init();

            Assert.AreEqual(new List<string> { null, ValidIp }, ipAddressOnNext);
            Assert.AreEqual(new List<string> { null, ValidPort }, portOnNext);
        }

        [Test]
        public void When_IpAddressChanged_Then_UpdatedIpAddressEmitted()
        {
            var ipAddressOnNext = new List<string>();
            _viewModel.IpAddress.Subscribe(value => ipAddressOnNext.Add(value));

            _viewModel.OnIpAddressChanged(ValidIp);

            Assert.AreEqual(new List<string> { null, ValidIp }, ipAddressOnNext);
        }

        [Test]
        public void When_OnPortChanged_Then_UpdatedPortEmitted()
        {
            var portOnNext = new List<string>();
            _viewModel.Port.Subscribe(value => portOnNext.Add(value));

            _viewModel.OnPortChanged(ValidPort);

            Assert.AreEqual(new List<string> { null, ValidPort }, portOnNext);
        }

        [Test]
        public void When_EnterOnlineDuelRoomButtonPressed_Then_OnlineDuelRoomUsed()
        {
            _viewModel.OnEnterOnlineDuelRoomPressed();

            _dataManager.Verify(dm => dm.SaveUseOnlineDuelRoom(true), Times.Once);
        }

        [Test]
        public void When_EnterOnlineDuelRoomButtonPressed_Then_DuelRoomShown()
        {
            _viewModel.OnEnterOnlineDuelRoomPressed();

            _navigationService.Verify(ns => ns.ShowDuelRoomScene(), Times.Once);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void When_SettingsMenuToggled_Then_ShowSettingsMenuEmitsUpdatedState(bool state)
        {
            var onNext = new List<bool>();
            _viewModel.ShowSettingsMenu.Subscribe(value => onNext.Add(value));

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
        public void When_DeveloperModeToggled_Then_ShowLocalConnectionMenuEmitsUpdatedState(bool state)
        {
            var onNext = new List<bool>();
            _viewModel.ShowLocalConnectionMenu.Subscribe(value => onNext.Add(value));

            _viewModel.OnDeveloperModeToggled(state);

            Assert.AreEqual(new List<bool> { false, state }, onNext);
        }
            
        [Test]
        public void Given_ValidForm_When_EnterLocalDuelRoomButtonPressed_Then_ConnectionInfoSaved()
        {
            var expected = new ConnectionInfo(ValidIp, ValidPort);
            _viewModel.OnIpAddressChanged(ValidIp);
            _viewModel.OnPortChanged(ValidPort);

            _viewModel.OnEnterLocalDuelRoomPressed();

            _dataManager.Verify(dm => dm.SaveConnectionInfo(expected), Times.Once);
        }

        [Test]
        public void Given_ValidForm_When_EnterLocalDuelRoomButtonPressed_Then_OfflineDuelRoomUsed()
        {
            _viewModel.OnIpAddressChanged(ValidIp);
            _viewModel.OnPortChanged(ValidPort);

            _viewModel.OnEnterLocalDuelRoomPressed();

            _dataManager.Verify(dm => dm.SaveUseOnlineDuelRoom(false), Times.Once);
        }

        [Test]
        public void Given_ValidForm_When_EnterLocalDuelRoomButtonPressed_Then_DuelRoomShown()
        {
            _viewModel.OnIpAddressChanged(ValidIp);
            _viewModel.OnPortChanged(ValidPort);

            _viewModel.OnEnterLocalDuelRoomPressed();

            _navigationService.Verify(ns => ns.ShowDuelRoomScene(), Times.Once);
        }

        [TestCase(null, ValidPort)]
        [TestCase(ValidIp, null)]
        [TestCase("Invalid", ValidPort)]
        [TestCase(ValidIp, "Invalid")]
        [Parallelizable]
        public void Given_InvalidForm_When_EnterLocalDuelRoomButtonPressed_Then_DuelRoomNotShown(string ip, string port)
        {
            _viewModel.OnIpAddressChanged(ip);
            _viewModel.OnPortChanged(port);
            _viewModel.OnEnterLocalDuelRoomPressed();

            _navigationService.Verify(ns => ns.ShowDuelRoomScene(), Times.Never);
        }

        [TestCase(null, ValidPort, LocaleKeys.ConnectionIPAddressRequired)]
        [TestCase(ValidIp, null, LocaleKeys.ConnectionPortRequired)]
        [TestCase("Invalid", ValidPort, LocaleKeys.ConnectionIPAddressInvalid)]
        [TestCase(ValidIp, "Invalid", LocaleKeys.ConnectionPortInvalid)]
        [Parallelizable]
        public void Given_InvalidForm_When_EnterLocalDuelRoomButtonPressed_Then_ErrorMessageShown(string ip, string port,
            string expected)
        {
            _viewModel.OnIpAddressChanged(ip);
            _viewModel.OnPortChanged(port);

            _viewModel.OnEnterLocalDuelRoomPressed();

            _dialogService.Verify(ds => ds.ShowToast(expected), Times.Once);
        }
    }
}