using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Code.Core.Config.Providers;
using Code.Core.DataManager;
using Code.Core.Dialog;
using Code.Core.Logger;
using Code.Core.Navigation;
using Code.Core.Screen;
using Code.Core.SmartDuelServer;
using Code.Core.SmartDuelServer.Entities;
using Code.Core.SmartDuelServer.Entities.EventData.CardEvents;
using Code.Core.SmartDuelServer.Entities.EventData.RoomEvents;
using Code.Features.DuelRoom;
using Code.Features.DuelRoom.Models;
using Editor.Tests.EditModeTests.Utils;
using Moq;
using NUnit.Framework;
using UniRx;

namespace Editor.Tests.EditModeTests.Features.DuelRoom
{
    public class DuelRoomViewModelTests
    {
        private DuelRoomViewModel _viewModel;

        private Mock<IDataManager> _dataManager;
        private Mock<INavigationService> _navigationService;
        private Mock<IDialogService> _dialogService;
        private Mock<ISmartDuelServer> _smartDuelServer;
        private Mock<IDelayProvider> _delayProvider;
        private Mock<IScreenService> _screenService;
        private Mock<IAppLogger> _logger;

        private Subject<SmartDuelEvent> _globalEvents;
        private Subject<SmartDuelEvent> _roomEvents;

        private const string ValidRoomName = "a1B2c";
        private const string ValidDuelistId = "duelistID";

        [SetUp]
        public void SetUp()
        {
            _dataManager = new Mock<IDataManager>();
            _navigationService = new Mock<INavigationService>();
            _dialogService = new Mock<IDialogService>();
            _smartDuelServer = new Mock<ISmartDuelServer>();
            _delayProvider = new Mock<IDelayProvider>();
            _screenService = new Mock<IScreenService>();
            _logger = new Mock<IAppLogger>();

            _globalEvents = new Subject<SmartDuelEvent>();
            _roomEvents = new Subject<SmartDuelEvent>();

            _smartDuelServer.SetupGet(sds => sds.GlobalEvents).Returns(_globalEvents);
            _smartDuelServer.SetupGet(sds => sds.RoomEvents).Returns(_roomEvents);

            _viewModel = new DuelRoomViewModel(
                _dataManager.Object,
                _navigationService.Object,
                _dialogService.Object,
                _smartDuelServer.Object,
                _delayProvider.Object,
                _screenService.Object,
                _logger.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _globalEvents.Dispose();
            _roomEvents.Dispose();
        }

        #region Initialization

        [Test]
        public void When_ViewModelInitialized_Then_LoadingStateEmitted()
        {
            var onNext = new List<DuelRoomState>();
            _viewModel.RoomState.Subscribe(value => onNext.Add(value));

            _viewModel.Init();

            Assert.AreEqual(new List<DuelRoomState> { DuelRoomState.Loading }, onNext);
        }

        [Test]
        public void When_ViewModelInitialized_Then_PortraitModeUsed()
        {
            _viewModel.Init();

            _screenService.Verify(ss => ss.UsePortraitOrientation(), Times.Once);
        }

        [Test]
        public void When_ViewModelInitialized_Then_StartListeningToSmartDuelEvents()
        {
            _viewModel.Init();

            _smartDuelServer.VerifyGet(sds => sds.GlobalEvents, Times.Once);
            _smartDuelServer.VerifyGet(sds => sds.RoomEvents, Times.Once);
        }

        [Test]
        public void When_ViewModelInitialized_Then_SmartDuelServerInitialized()
        {
            _viewModel.Init();

            _smartDuelServer.Verify(sds => sds.Init(), Times.Once);
        }

        #endregion

        #region Button Events

        [Test]
        public void Given_ValidRoomName_When_EnterRoomButtonPressed_Then_GetDuelistsInRoomEventSent()
        {
            var expected = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomGetDuelistsAction,
                new RoomEventData
                {
                    RoomName = ValidRoomName
                });
            _viewModel.UpdateRoomName(ValidRoomName);

            _viewModel.OnEnterRoomPressed();

            _smartDuelServer.Verify(sds => sds.EmitEvent(expected), Times.Once);
        }

        [TestCase("")]
        [TestCase("     ")]
        [TestCase(null)]
        [Parallelizable]
        public void Given_InvalidRoomName_When_EnterRoomButtonPressed_Then_ErrorMessageShown(string invalidName)
        {
            _viewModel.UpdateRoomName(invalidName);

            _viewModel.OnEnterRoomPressed();

            _dialogService.Verify(ds => ds.ShowToast("Room name is required"), Times.Once);
        }

        [Test]
        public void Given_ValidDuelistId_When_SpectateButtonPressed_Then_SpectateRoomEventSent()
        {
            _viewModel.UpdateRoomName(ValidRoomName);
            
            var expected = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomSpectateAction,
                new RoomEventData
                {
                    RoomName = ValidRoomName
                });

            _viewModel.OnSpectateButtonPressed(ValidDuelistId);

            _smartDuelServer.Verify(sds => sds.EmitEvent(expected), Times.Once);
        }

        [TestCase("")]
        [TestCase("     ")]
        [TestCase(null)]
        [Parallelizable]
        public void Given_InvalidDuelistId_When_SpectateButtonPressed_Then_ErrorMessageShown(string invalidDuelistId)
        {
            _viewModel.OnSpectateButtonPressed(invalidDuelistId);

            _dialogService.Verify(ds => ds.ShowToast("Duelist name is required"), Times.Once);
        }

        [Test]
        public void When_GoBackButtonPressed_Then_EnterRoomNameStateEmitted()
        {
            var expected = new List<DuelRoomState> { DuelRoomState.Loading, DuelRoomState.EnterRoomName };
            var onNext = new List<DuelRoomState>();
            _viewModel.RoomState.Subscribe(value => onNext.Add(value));

            _viewModel.OnGoBackButtonPressed();

            Assert.AreEqual(expected, onNext);
        }

        [Test]
        public void When_GoBackButtonPressed_Then_RoomNameCleared()
        {
            var onNext = new List<string>();
            _viewModel.RoomName.Subscribe(value => onNext.Add(value));

            _viewModel.OnGoBackButtonPressed();

            Assert.AreEqual(new List<string> { null, null }, onNext);
        }

        [Test]
        public void When_GoBackButtonPressed_Then_DuelistIdsCleared()
        {
            var onNext = new List<List<string>>();
            _viewModel.DuelistIds.Subscribe(value => onNext.Add(value));

            _viewModel.OnGoBackButtonPressed();

            Assert.AreEqual(new List<List<string>> { null, null }, onNext);
        }
        
        [Test]
        public void When_GoBackButtonPressed_Then_LeaveRoomEventSent()
        {
            var expected = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomLeaveAction);

            _viewModel.OnGoBackButtonPressed();

            _smartDuelServer.Verify(sds => sds.EmitEvent(expected), Times.Once);
        }

        [Test]
        public void When_TryAgainPressed_Then_LoadingStateEmitted()
        {
            var expected = new List<DuelRoomState> { DuelRoomState.Loading, DuelRoomState.Loading };
            var onNext = new List<DuelRoomState>();
            _viewModel.RoomState.Subscribe(value => onNext.Add(value));

            TestUtils.RunAsyncMethodSync(() => _viewModel.OnTryAgainButtonPressed());

            Assert.AreEqual(expected, onNext);
        }

        [Test]
        public void When_TryAgainPressed_Then_SmartDuelServerRestarted()
        {
            TestUtils.RunAsyncMethodSync(() => _viewModel.OnTryAgainButtonPressed());

            _smartDuelServer.Verify(sds => sds.Dispose(), Times.Once);
            _smartDuelServer.Verify(sds => sds.Init(), Times.Once);
        }

        [Test]
        public void When_TryAgainButtonPressed_Then_DuelistIdsCleared()
        {
            var onNext = new List<List<string>>();
            _viewModel.DuelistIds.Subscribe(value => onNext.Add(value));

            TestUtils.RunAsyncMethodSync(() => _viewModel.OnTryAgainButtonPressed());

            Assert.AreEqual(new List<List<string>> { null, null }, onNext);
        }

        [Test]
        public void When_TryAgainButtonPressed_Then_RoomNameCleared()
        {
            var onNext = new List<string>();
            _viewModel.RoomName.Subscribe(value => onNext.Add(value));

            TestUtils.RunAsyncMethodSync(() => _viewModel.OnTryAgainButtonPressed());

            Assert.AreEqual(new List<string> { null, null }, onNext);
        }

        [Test]
        public void When_LeaveRoomButtonPressed_Then_EnterRoomStateEmitted()
        {
            var expected = new List<DuelRoomState> { DuelRoomState.Loading, DuelRoomState.EnterRoomName };
            var onNext = new List<DuelRoomState>();
            _viewModel.RoomState.Subscribe(value => onNext.Add(value));

            _viewModel.OnLeaveRoomButtonPressed();

            Assert.AreEqual(expected, onNext);
        }

        [Test]
        public void When_LeaveRoomButtonPressed_Then_LeaveRoomEventSent()
        {
            var expected = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomLeaveAction);

            _viewModel.OnLeaveRoomButtonPressed();

            _smartDuelServer.Verify(sds => sds.EmitEvent(expected), Times.Once);
        }

        [Test]
        public void When_LeaveRoomButtonPressed_Then_DuelistIdsCleared()
        {
            var onNext = new List<List<string>>();
            _viewModel.DuelistIds.Subscribe(value => onNext.Add(value));

            _viewModel.OnLeaveRoomButtonPressed();

            Assert.AreEqual(new List<List<string>> { null, null }, onNext);
        }

        [Test]
        public void When_LeaveRoomButtonPressed_Then_RoomNameCleared()
        {
            var onNext = new List<string>();
            _viewModel.RoomName.Subscribe(value => onNext.Add(value));

            _viewModel.OnLeaveRoomButtonPressed();

            Assert.AreEqual(new List<string> { null, null }, onNext);
        }

        #endregion

        #region Form Fields

        [Test]
        public void When_UpdateRoomName_Then_RoomNameEmitted()
        {
            var onNext = new List<string>();
            _viewModel.RoomName.Subscribe(value => onNext.Add(value));

            _viewModel.UpdateRoomName(ValidRoomName);

            Assert.AreEqual(new List<string> { null, ValidRoomName }, onNext);
        }

        #endregion

        #region Receive Global Event

        [Test]
        public void When_GlobalConnectEventReceived_Then_EnterRoomNameStatEmitted()
        {
            var expected = new List<DuelRoomState> { DuelRoomState.Loading, DuelRoomState.EnterRoomName };
            var onNext = new List<DuelRoomState>();
            _viewModel.RoomState.Subscribe(value => onNext.Add(value));

            var globalConnectEvent = new SmartDuelEvent(
                SmartDuelEventConstants.GlobalScope,
                SmartDuelEventConstants.GlobalConnectAction);

            _viewModel.OnSmartDuelEventReceived(globalConnectEvent);

            Assert.AreEqual(expected, onNext);
        }

        [TestCase(SmartDuelEventConstants.GlobalConnectErrorAction)]
        [TestCase(SmartDuelEventConstants.GlobalConnectTimeoutAction)]
        [TestCase(SmartDuelEventConstants.GlobalErrorAction)]
        [Parallelizable]
        public void When_GlobalErrorEventReceived_Then_ErrorStateEmitted(string error)
        {
            var expected = new List<DuelRoomState> { DuelRoomState.Loading, DuelRoomState.Error };
            var onNext = new List<DuelRoomState>();
            _viewModel.RoomState.Subscribe(value => onNext.Add(value));

            var globalErrorEvent = new SmartDuelEvent(
                SmartDuelEventConstants.GlobalScope,
                error);

            _viewModel.OnSmartDuelEventReceived(globalErrorEvent);

            Assert.AreEqual(expected, onNext);
        }

        [TestCase(SmartDuelEventConstants.GlobalConnectErrorAction)]
        [TestCase(SmartDuelEventConstants.GlobalConnectTimeoutAction)]
        [TestCase(SmartDuelEventConstants.GlobalErrorAction)]
        [Parallelizable]
        public void When_GlobalErrorEventReceived_Then_ErrorMessageEmitted(string expectedError)
        {
            var onNext = new List<string>();
            _viewModel.ErrorText.Subscribe(value => onNext.Add(value));

            var globalErrorEvent = new SmartDuelEvent(
                SmartDuelEventConstants.GlobalScope,
                expectedError);

            _viewModel.OnSmartDuelEventReceived(globalErrorEvent);

            Assert.AreEqual(new List<string> { null, expectedError }, onNext);
        }

        #endregion

        #region Receive Room Event

        [Test]
        public void Given_NoRoomEventData_When_RoomEventReceived_ErrorStateReturned()
        {
            var expected = new List<DuelRoomState> { DuelRoomState.Loading, DuelRoomState.Error };
            var onNext = new List<DuelRoomState>();
            _viewModel.RoomState.Subscribe(value => onNext.Add(value));

            var invalidRoomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomCloseAction);

            _viewModel.OnSmartDuelEventReceived(invalidRoomEvent);

            Assert.AreEqual(expected, onNext);
        }

        [Test]
        public void Given_NoRoomEventData_When_RoomEventReceived_ErrorMessageEmitted()
        {
            var onNext = new List<string>();
            _viewModel.ErrorText.Subscribe(value => onNext.Add(value));

            var invalidRoomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomCloseAction);

            _viewModel.OnSmartDuelEventReceived(invalidRoomEvent);

            Assert.AreEqual(new List<string> { null, "Room Data is Invalid!" }, onNext);
        }

        [Test]
        public void Given_RoomGetDuelistsAction_And_ErrorIsNotNull_When_RoomEventReceived_Then_ErrorStateEmitted()
        {
            var expected = new List<DuelRoomState> { DuelRoomState.Loading, DuelRoomState.Error };
            var onNext = new List<DuelRoomState>();
            _viewModel.RoomState.Subscribe(value => onNext.Add(value));

            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomGetDuelistsAction,
                new RoomEventData { Error = "Error" });

            _viewModel.OnSmartDuelEventReceived(roomEvent);

            Assert.AreEqual(expected, onNext);
        }

        [Test]
        public void Given_RoomGetDuelistsAction_And_ErrorIsNotNull_When_RoomEventReceived_Then_ErrorTextReturned()
        {
            const string errorMessage = "Error";
            var onNext = new List<string>();
            _viewModel.ErrorText.Subscribe(value => onNext.Add(value));

            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomGetDuelistsAction,
                new RoomEventData { Error = errorMessage });

            _viewModel.OnSmartDuelEventReceived(roomEvent);

            Assert.AreEqual(new List<string> { null, errorMessage }, onNext);
        }

        [Test]
        public void Given_RoomGetDuelistsAction_And_ErrorIsNotNull_When_RoomEventReceived_Then_SelectDuelistStateEmitted()
        {
            var expected = new List<DuelRoomState> { DuelRoomState.Loading, DuelRoomState.SelectDuelist };
            var onNext = new List<DuelRoomState>();
            _viewModel.RoomState.Subscribe(value => onNext.Add(value));

            var data = new RoomEventData { DuelistsIds = new List<string> { "validID", "validID2" } };
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomGetDuelistsAction,
                data);

            _viewModel.OnSmartDuelEventReceived(roomEvent);

            Assert.AreEqual(expected, onNext);
        }

        [Test]
        public void Given_RoomGetDuelistsAction_And_ErrorIsNotNull_When_RoomEventReceived_Then_DuelistIdsEmitted()
        {
            var duelistIds = new List<string> { "validID", "validID2" };
            var onNext = new List<List<string>>();
            _viewModel.DuelistIds.Subscribe(value => onNext.Add(value));

            var data = new RoomEventData { DuelistsIds = duelistIds };
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomGetDuelistsAction,
                data);

            _viewModel.OnSmartDuelEventReceived(roomEvent);

            Assert.AreEqual(new List<List<string>> { null, duelistIds }, onNext);
        }
        
        [Test]
        public void Given_RoomGetDuelistsAction_And_PreviousDuelistIdsAvailable_When_RoomEventReceived_Then_ToastMessageShown()
        {
            var duelistIds = new List<string> { "validID", "validID2" };

            var data = new RoomEventData { DuelistsIds = duelistIds };
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomGetDuelistsAction,
                data);

            _viewModel.OnSmartDuelEventReceived(roomEvent);
            _viewModel.OnSmartDuelEventReceived(roomEvent);

            _dialogService.Verify(ds => ds.ShowToast("A new duelist has appeared!"), Times.Once);
        }

        [Test]
        public void Given_RoomSpectateAction_When_RoomEventReceived_Then_WaitingStateEmitted()
        {
            var expected = new List<DuelRoomState> { DuelRoomState.Loading, DuelRoomState.Waiting };
            var onNext = new List<DuelRoomState>();
            _viewModel.RoomState.Subscribe(value => onNext.Add(value));

            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomSpectateAction,
                new RoomEventData());

            _viewModel.OnSmartDuelEventReceived(roomEvent);

            Assert.AreEqual(expected, onNext);
        }

        [Test]
        public void Given_RoomStartAction_When_RoomEventReceived_Then_DuelRoomSaved()
        {
            var room = new RoomEventData { DuelRoom = new Code.Core.SmartDuelServer.Entities.EventData.RoomEvents.DuelRoom() };
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomStartAction,
                room);

            _viewModel.OnSmartDuelEventReceived(roomEvent);

            _dataManager.Verify(dm => dm.SaveDuelRoom(room.DuelRoom), Times.Once);
        }

        [Test]
        public void Given_RoomStartAction_When_RoomEventReceived_Then_SpeedDuelSceneShown()
        {
            var room = new RoomEventData { DuelRoom = new Code.Core.SmartDuelServer.Entities.EventData.RoomEvents.DuelRoom() };
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomStartAction,
                room);

            _viewModel.OnSmartDuelEventReceived(roomEvent);

            _navigationService.Verify(ns => ns.ShowSpeedDuelScene(), Times.Once);
        }

        [Test]
        public void Given_RoomStartAction_When_RoomEventReceived_Then_LoadingStateEmitted()
        {
            var expected = new List<DuelRoomState> { DuelRoomState.Loading, DuelRoomState.Loading };
            var onNext = new List<DuelRoomState>();
            _viewModel.RoomState.Subscribe(value => onNext.Add(value));

            var room = new RoomEventData { DuelRoom = new Code.Core.SmartDuelServer.Entities.EventData.RoomEvents.DuelRoom() };
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomStartAction,
                room);

            _viewModel.OnSmartDuelEventReceived(roomEvent);

            Assert.AreEqual(expected, onNext);
        }

        [Test]
        public void Given_RoomStartAction_And_NoDuelRoom_When_RoomEventReceived_Then_ErrorStateEmitted()
        {
            var expected = new List<DuelRoomState> { DuelRoomState.Loading, DuelRoomState.Error };
            var onNext = new List<DuelRoomState>();
            _viewModel.RoomState.Subscribe(value => onNext.Add(value));

            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomStartAction,
                new RoomEventData());

            _viewModel.OnSmartDuelEventReceived(roomEvent);

            Assert.AreEqual(expected, onNext);
        }

        [Test]
        public void Given_RoomStartAction_And_NoDuelRoom_When_RoomEventReceived_Then_ErrorTextReturned()
        {
            var onNext = new List<string>();
            _viewModel.ErrorText.Subscribe(value => onNext.Add(value));

            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomStartAction,
                new RoomEventData());

            _viewModel.OnSmartDuelEventReceived(roomEvent);

            Assert.AreEqual(new List<string> { null, "No duel room data found" }, onNext);
        }

        [Test]
        public void Given_RoomCloseAction_When_RoomEventReceived_Then_ErrorStateEmitted()
        {
            var expected = new List<DuelRoomState> { DuelRoomState.Loading, DuelRoomState.Error };
            var onNext = new List<DuelRoomState>();
            _viewModel.RoomState.Subscribe(value => onNext.Add(value));

            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomCloseAction,
                new RoomEventData());

            _viewModel.OnSmartDuelEventReceived(roomEvent);

            Assert.AreEqual(expected, onNext);
        }

        [Test]
        public void Given_RoomCloseAction_When_RoomEventReceived_Then_ErrorTextEmitted()
        {
            var onNext = new List<string>();
            _viewModel.ErrorText.Subscribe(value => onNext.Add(value));

            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomCloseAction,
                new RoomEventData());

            _viewModel.OnSmartDuelEventReceived(roomEvent);

            Assert.AreEqual(new List<string> { null, "Duel room is closed" }, onNext);
        }

        #endregion

        #region Clean-up

        [Test]
        public void Given_DuelStarted_When_DisposeCalled_ServerNotDisposed()
        {
            var room = new RoomEventData { DuelRoom = new Code.Core.SmartDuelServer.Entities.EventData.RoomEvents.DuelRoom() };
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomStartAction,
                room);
            _viewModel.OnSmartDuelEventReceived(roomEvent);

            _viewModel.Dispose();

            _smartDuelServer.Verify(sds => sds.Dispose(), Times.Never);
        }

        [Test]
        public void Given_DuelNotStarted_When_DisposeCalled_ServerDisposed()
        {
            _viewModel.Dispose();

            _smartDuelServer.Verify(sds => sds.Dispose(), Times.Once);
        }

        #endregion
    }
}