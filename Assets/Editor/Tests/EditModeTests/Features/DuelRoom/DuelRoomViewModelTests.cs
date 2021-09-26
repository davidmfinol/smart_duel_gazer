using Code.Core.Config.Providers;
using Code.Core.DataManager;
using Code.Core.Dialog;
using Code.Core.Logger;
using Code.Core.Navigation;
using Code.Core.Screen;
using Code.Core.SmartDuelServer;
using Code.Core.SmartDuelServer.Entities;
using Code.Core.SmartDuelServer.Entities.EventData.RoomEvents;
using Code.Features.DuelRoom;
using Code.Features.DuelRoom.Models;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using UniRx;
using Tests.Utils;
using Code.Core.SmartDuelServer.Entities.EventData.CardEvents;

namespace Tests.Features.DuelRoom
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

        private const string _validRoomName = "a1B2c";
        private const string _validDuelistId = "duelistID";

        private DuelRoomState _expectedState;
        private string _expectedErrorText;

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

            _viewModel = new DuelRoomViewModel(
                _dataManager.Object,
                _navigationService.Object,
                _dialogService.Object,
                _smartDuelServer.Object,
                _delayProvider.Object,
                _screenService.Object,
                _logger.Object);

            _viewModel.UpdateDuelRoomState.Subscribe(e => { _expectedState = e; });
            _viewModel.UpdateErrorTextField.Subscribe(e => { _expectedErrorText = e; });
        }

        #region Button Logic Tests

        [Test]
        //TODO: Ensure proper room name is passed
        public void Given_AValidRoomName_When_EnterRoomButtonPressed_Then_SendDuelistsInRoomEventCalled()
        {
            _viewModel.UpdateRoomName(_validRoomName);
            
            _viewModel.OnEnterRoomPressed();

            _smartDuelServer.Verify(sds => sds.EmitEvent(It.Is<SmartDuelEvent>(
                sde => sde.Action == SmartDuelEventConstants.RoomGetDuelistsAction)), Times.Once);
            _smartDuelServer.Verify(sds => sds.EmitEvent(It.Is<SmartDuelEvent>(
                sde => sde.Data is RoomEventData)), Times.Once);
        }

        [TestCase("")]
        [TestCase("     ")]
        [TestCase(null)]
        [Parallelizable()]
        public void Given_InvalidRoomName_When_EnterRoomButtonPressed_Then_ToastErrorShown(string invalidName)
        {
            _viewModel.UpdateRoomName(invalidName);
            
            _viewModel.OnEnterRoomPressed();

            _dialogService.Verify(ds => ds.ShowToast("Room name is required"), Times.Once);
            _smartDuelServer.Verify(sds => sds.EmitEvent(It.IsAny<SmartDuelEvent>()), Times.Never);
        }

        [Test]
        public void Given_ValidDuelists_When_SpectateButtonPressed_Then_SpectateRoomEventSent()
        {
            _viewModel.OnSpectateButtonPressed(_validDuelistId);

            _smartDuelServer.Verify(sds => sds.EmitEvent(It.Is<SmartDuelEvent>(
                e => e.Action == SmartDuelEventConstants.RoomSpectateAction)), Times.Once);
        }

        [TestCase("")]
        [TestCase("     ")]
        [TestCase(null)]
        [Parallelizable()]
        public void Given_InvalidDuelists_When_SpectateButtonPressed_Then_SpectateRoomEventSent(string invalidDuelistId)
        {
            _viewModel.OnSpectateButtonPressed(invalidDuelistId);

            _dialogService.Verify(ds => ds.ShowToast("Duelist name is required"), Times.Once);
            _smartDuelServer.Verify(sds => sds.EmitEvent(It.IsAny<SmartDuelEvent>()), Times.Never);
        }

        [Test]
        public void When_GoBackButtonPressed_Then_DuelRoomStateIsEnterRoomName()
        {
            var model = DuelRoomState.EnterRoomName;

            _viewModel.OnGoBackButtonPressed();

            Assert.AreEqual(model, _expectedState);            
        }

        [Test]
        public void When_TryAgainPressed_Then_ServerResarts()
        {
            TestUtils.RunAsyncMethodSync(() => _viewModel.OnTryAgainButtonPressed());

            _smartDuelServer.Verify(sds => sds.Init(), Times.Once);
        }

        [Test]
        public void When_TryAgainPressed_Then_LoadingStateReturned()
        {
            var model = DuelRoomState.Loading;

            TestUtils.RunAsyncMethodSync(() => _viewModel.OnTryAgainButtonPressed());

            Assert.AreEqual(model, _expectedState);
        }

        [Test]
        public void When_LeaveRoomButtonPressed_Then_LeaveRoomEventFired()
        {
            _viewModel.OnLeaveRoomButtonPressed();

            _smartDuelServer.Verify(sds => sds.EmitEvent(It.Is<SmartDuelEvent>(
                sde => sde.Action == SmartDuelEventConstants.RoomLeaveAction)), Times.Once);
        }

        [Test]
        public void When_LeaveRoomButtonPressed_Then_EnterRoomStateReturned()
        {
            var model = DuelRoomState.EnterRoomName;

            _viewModel.OnLeaveRoomButtonPressed();

            Assert.AreEqual(model, _expectedState);
        }

        #endregion

        #region Global Event Logic Tests

        [Test]
        public void When_GlobalConnectEvent_Then_EnterRoomNameStatReturned()
        {
            var model = DuelRoomState.EnterRoomName;
            var globalConnectEvent = new SmartDuelEvent(
                SmartDuelEventConstants.GlobalScope,
                SmartDuelEventConstants.GlobalConnectAction,
                null);

            _viewModel.OnSmartDuelEventReceived(globalConnectEvent);

            Assert.AreEqual(model, _expectedState);
        }

        [TestCase(SmartDuelEventConstants.GlobalConnectErrorAction)]
        [TestCase(SmartDuelEventConstants.GlobalConnectTimeoutAction)]
        [TestCase(SmartDuelEventConstants.GlobalErrorAction)]
        [Parallelizable()]
        public void When_GlobalErrorEvent_Then_ErrorMessageAppears(string error)
        {
            var model = DuelRoomState.Error;
            var globalErrorEvent = new SmartDuelEvent(
                SmartDuelEventConstants.GlobalScope,
                error,
                null);

            _viewModel.OnSmartDuelEventReceived(globalErrorEvent);

            Assert.AreEqual(model, _expectedState);
            Assert.AreEqual(error, _expectedErrorText);
        }

        #endregion

        #region Room Event Logic Tests

        [TestCase(SmartDuelEventConstants.RoomGetDuelistsAction)]
        [TestCase(SmartDuelEventConstants.RoomLeaveAction)]
        [TestCase(SmartDuelEventConstants.RoomSpectateAction)]
        [TestCase(SmartDuelEventConstants.RoomStartAction)]
        [TestCase(SmartDuelEventConstants.RoomCloseAction)]
        [Parallelizable()]
        public void Given_InvalidEventData_When_Handled_ErrorStateIsReturned(string eventName)
        {
            var model = DuelRoomState.Error;
            RoomEventData invalidEventArgs = null;
            var invalidRoomEventData = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                eventName,
                invalidEventArgs);

            _viewModel.OnSmartDuelEventReceived(invalidRoomEventData);

            Assert.AreEqual(model, _expectedState);
        }

        [TestCase(SmartDuelEventConstants.RoomGetDuelistsAction)]
        [TestCase(SmartDuelEventConstants.RoomLeaveAction)]
        [TestCase(SmartDuelEventConstants.RoomSpectateAction)]
        [TestCase(SmartDuelEventConstants.RoomStartAction)]
        [TestCase(SmartDuelEventConstants.RoomCloseAction)]
        [Parallelizable()]
        public void Given_InvalidRoomEvent_When_Handled_ErrorIsReturned(string eventName)
        {
            var model = DuelRoomState.Error;
            CardEventData invalidEventArgs = null;
            var invalidRoomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                eventName,
                invalidEventArgs);

            _viewModel.OnSmartDuelEventReceived(invalidRoomEvent);

            Assert.AreEqual(model, _expectedState);
            Assert.AreEqual("Room Data is Invalid!", _expectedErrorText);
        }

        [TestCase(SmartDuelEventConstants.GlobalConnectErrorAction)]
        [TestCase(SmartDuelEventConstants.GlobalConnectTimeoutAction)]
        [TestCase(SmartDuelEventConstants.GlobalErrorAction)]
        [Parallelizable()]
        public void Given_RoomEventError_When_GetDuelistsEventCalled_Then_ReturnErrorState(string error)
        {
            var model = DuelRoomState.Error;
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomGetDuelistsAction,
                new RoomEventData { Error = error });

            _viewModel.OnSmartDuelEventReceived(roomEvent);

            Assert.AreEqual(model, _expectedState);
            Assert.AreEqual(error, _expectedErrorText);
        }

        [Test]
        public void Given_ValidDuelistID_When_GetDuelistsEventCalled_Then_ReturnSelectDuelistState()
        {
            var model = DuelRoomState.SelectDuelist;
            var list = new List<string>();
            list.Add("validID");
            list.Add("validID2");
            var data = new RoomEventData { DuelistsIds = list };
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomGetDuelistsAction,
                data);

            _viewModel.OnSmartDuelEventReceived(roomEvent);

            Assert.AreEqual(model, _expectedState);
        }

        [Test]
        public void When_RoomSpectateEventRecieved_Then_WaitingDuelRoomStateIsReturned()
        {
            var model = DuelRoomState.Waiting;
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomSpectateAction,
                new RoomEventData());

            _viewModel.OnSmartDuelEventReceived(roomEvent);

            Assert.AreEqual(model, _expectedState);
        }

        [Test]
        public void When_RoomStartEvent_Then_RoomSavedAndSpeedDuelSceneShown()
        {
            var room = new RoomEventData { DuelRoom = new Code.Core.SmartDuelServer.Entities.EventData.RoomEvents.DuelRoom() };
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomStartAction,
                room);

            _viewModel.OnSmartDuelEventReceived(roomEvent);

            _dataManager.Verify(dm => dm.SaveDuelRoom(room.DuelRoom), Times.Once);
            _navigationService.Verify(ns => ns.ShowSpeedDuelScene(), Times.Once);
        }

        [Test]
        public void Given_NullRoom_When_RoomStartEvent_Then_ErrorStateReturned()
        {
            var model = DuelRoomState.Error;
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomStartAction,
                new RoomEventData());

            _viewModel.OnSmartDuelEventReceived(roomEvent);

            _navigationService.Verify(ns => ns.ShowSpeedDuelScene(), Times.Never);
            Assert.AreEqual(model, _expectedState);
        }

        [Test]
        public void When_RoomClosedEventRecieved_Then_ErrorDuelRoomStateIsReturned()
        {
            var model = DuelRoomState.Error;
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomCloseAction,
                new RoomEventData());

            _viewModel.OnSmartDuelEventReceived(roomEvent);

            Assert.AreEqual(model, _expectedState);
            Assert.AreEqual("roomIsClosed", _expectedErrorText);
        }

        #endregion
    }
}