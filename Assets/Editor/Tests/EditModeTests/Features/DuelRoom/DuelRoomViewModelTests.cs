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

namespace Tests.Features.DuelRoom
{
    // TODO: Update Unit Tests
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

        }

        #region Button Logic Tests

        [Test]
        public void Given_AValidRoomName_When_EnterRoomButtonPressed_Then_SendDuelistsInRoomEventCalled()
        {
            _viewModel.OnEnterRoomPressed();

            _smartDuelServer.Verify(sds => sds.EmitEvent(It.Is<SmartDuelEvent>(
                sde => sde.Action == SmartDuelEventConstants.RoomGetDuelistsAction)), Times.Once);
        }

        [Test]
        [TestCase("")]
        [TestCase("     ")]
        [TestCase(null)]
        [Parallelizable]
        public void Given_InvalidRoomName_When_EnterRoomButtonPressed_Then_ToastErrorShown(string invalidName)
        {
            _viewModel.OnEnterRoomPressed();

            _dialogService.Verify(ds => ds.ShowToast("Room name is required"), Times.Once);
            _smartDuelServer.Verify(sds => sds.EmitEvent(It.IsAny<SmartDuelEvent>()), Times.Never);
        }

        [Test]
        public void Given_ValidDuelists_When_SpectateButtonPressed_Then_SpectateRoomEventSent()
        {
            _viewModel.OnSpectateButtonPressed("duelistID");

            _smartDuelServer.Verify(sds => sds.EmitEvent(It.Is<SmartDuelEvent>(
                e => e.Action == SmartDuelEventConstants.RoomSpectateAction)), Times.Once);
        }

        [Test]
        [TestCase("")]
        [TestCase("     ")]
        [TestCase(null)]
        [Parallelizable]
        public void Given_InvalidDuelists_When_SpectateButtonPressed_Then_SpectateRoomEventSent(string value)
        {
            _viewModel.OnSpectateButtonPressed(value);

            _dialogService.Verify(ds => ds.ShowToast("Duelist name is required"), Times.Once);
            _smartDuelServer.Verify(sds => sds.EmitEvent(It.IsAny<SmartDuelEvent>()), Times.Never);
        }

        [Test]
        public void When_GoBackButtonPressed_Then_DuelRoomStateIsEnterRoomName()
        {
            //var model = DuelRoomState.EnterRoomName;

            _viewModel.OnGoBackButtonPressed();

            //Assert.AreEqual(model, expected);
        }

        [Test]
        public void When_TryAgainPressed_Then_ServerResartsAndLoadingStateReturned()
        {
            //var model = DuelRoomState.Loading;

            //_viewModel.OnTryAgainButtonPressed();

            _smartDuelServer.Verify(sds => sds.Init(), Times.Once);
            //Assert.AreEqual(model, expected);
        }

        [Test]
        public void When_LeaveRoomButtonPressed_Then_LeaveRoomEventFiredAndEnterRoomStateReturned()
        {
            //var model = DuelRoomState.EnterRoomName;

            _viewModel.OnLeaveRoomButtonPressed();

            _smartDuelServer.Verify(sds => sds.EmitEvent(It.Is<SmartDuelEvent>(
                sde => sde.Action == SmartDuelEventConstants.RoomLeaveAction)), Times.Once);
            //Assert.AreEqual(model, expected);
        }

        #endregion

        #region Global Event Logic Tests

        [Test]
        public void When_GlobalConnectEvent_Then_EnterRoomNameStatReturned()
        {
            //var model = DuelRoomState.EnterRoomName;
            var globalEvent = new SmartDuelEvent(
                SmartDuelEventConstants.GlobalScope,
                SmartDuelEventConstants.GlobalConnectAction,
                null);

            _viewModel.HandleGlobalEvent(globalEvent);

            //Assert.AreEqual(model, expected);
        }

        [Test]
        [TestCase(SmartDuelEventConstants.GlobalConnectErrorAction)]
        [TestCase(SmartDuelEventConstants.GlobalConnectTimeoutAction)]
        [TestCase(SmartDuelEventConstants.GlobalErrorAction)]
        [Parallelizable]
        public void When_GlobalErrorEvent_Then_ErrorMessageAppears(string error)
        {
            //var model = DuelRoomState.Error;
            var globalEvent = new SmartDuelEvent(
                SmartDuelEventConstants.GlobalScope,
                error,
                null);

            _viewModel.HandleGlobalEvent(globalEvent);

            //Assert.AreEqual(model, expected);
        }

        #endregion

        #region Room Event Logic Tests

        [Test]
        [TestCase(SmartDuelEventConstants.RoomGetDuelistsAction)]
        [TestCase(SmartDuelEventConstants.RoomLeaveAction)]
        [TestCase(SmartDuelEventConstants.RoomSpectateAction)]
        [TestCase(SmartDuelEventConstants.RoomStartAction)]
        [TestCase(SmartDuelEventConstants.RoomCloseAction)]
        [Parallelizable]
        public void Given_AnInvalidEvent_When_Handled_ErrorStateIsReturned(string eventName)
        {
            //var model = DuelRoomState.Error;
            RoomEventData invalidEventArgs = null;
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                eventName,
                invalidEventArgs);

            _viewModel.HandleRoomEvent(roomEvent);

            //Assert.AreEqual(model, expected);
        }

        [Test]
        [TestCase(SmartDuelEventConstants.GlobalConnectErrorAction)]
        [TestCase(SmartDuelEventConstants.GlobalConnectTimeoutAction)]
        [TestCase(SmartDuelEventConstants.GlobalErrorAction)]
        [Parallelizable]
        public void Given_ARoomEventError_When_GetDuelistsEventCalled_Then_ReturnErrorState(string error)
        {
            //var model = DuelRoomState.Error;
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomGetDuelistsAction,
                new RoomEventData { Error = error });

            _viewModel.HandleRoomEvent(roomEvent);

            //Assert.AreEqual(model, expected);
        }

        [Test]
        public void Given_ValidDuelistID_When_GetDuelistsEventCalled_Then_ReturnSelectDuelistState()
        {
            //var model = DuelRoomState.SelectDuelist;
            var list = new List<string>();
            list.Add("validID");
            list.Add("validID2");

            var data = new RoomEventData { DuelistsIds = list };
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomGetDuelistsAction,
                data);

            _viewModel.HandleRoomEvent(roomEvent);

            //Assert.AreEqual(model, expected);
        }

        [Test]
        public void When_RoomSpectateEventRecieved_Then_SpectateDuelRoomStateIsReturned()
        {
            //var model = DuelRoomState.Waiting;
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomSpectateAction,
                new RoomEventData());

            _viewModel.HandleRoomEvent(roomEvent);

            //Assert.AreEqual(model, expected);
        }

        [Test]
        public void When_RoomStartEvent_Then_RoomSavedAndSpeedDuelSceneShown()
        {
            var room = new RoomEventData { DuelRoom = new Code.Core.SmartDuelServer.Entities.EventData.RoomEvents.DuelRoom() };
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomStartAction,
                room);

            _viewModel.HandleRoomEvent(roomEvent);

            _dataManager.Verify(dm => dm.SaveDuelRoom(room.DuelRoom), Times.Once);
            _navigationService.Verify(ns => ns.ShowSpeedDuelScene(), Times.Once);
        }

        [Test]
        public void Given_ANullRoom_When_RoomStartEvent_Then_ErrorStateReturned()
        {
            //var model = DuelRoomState.Error;
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomStartAction,
                new RoomEventData());

            _viewModel.HandleRoomEvent(roomEvent);

            _navigationService.Verify(ns => ns.ShowSpeedDuelScene(), Times.Never);
            //Assert.AreEqual(model, expected);
        }

        [Test]
        public void When_RoomClosedEventRecieved_Then_ErrorDuelRoomStateIsReturned()
        {
            //var model = DuelRoomState.Error;
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomCloseAction,
                new RoomEventData());

            _viewModel.HandleRoomEvent(roomEvent);

            //Assert.AreEqual(model, expected);
        }

        #endregion
    }
}