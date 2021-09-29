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
using NUnit.Framework.Internal;

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

        private Randomizer _randomizer = new Randomizer();
        
        private const string _validRoomName = "a1B2c";
        private const string _validDuelistId = "duelistID";

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

        // TODO: Null Reference Exception thrown when running these tests
        #region Initialization

        [Test]
        public void When_ViewModelInitialized_Then_PortraitModeUsed()
        {
            _viewModel.Init();

            _screenService.Verify(ss => ss.UsePortraitOrientation(), Times.Once);
        }

        [Test]
        public void When_ViewModelInitialized_Then_SmartDuelServerInitialized()
        {
            _viewModel.Init();

            _smartDuelServer.Verify(sds => sds.Init(), Times.Once);
        }

        #endregion

        #region Button Logic Tests

        [Test]
        //TODO: Ensure proper room name is passed
        public void Given_AValidRoomName_When_EnterRoomButtonPressed_Then_SendDuelistsInRoomEventCalled()
        {
            _viewModel.UpdateRoomName(_validRoomName);
            
            _viewModel.OnEnterRoomPressed();

            _smartDuelServer.Verify(sds => sds.EmitEvent(It.Is<SmartDuelEvent>(
                sde => sde.Action == SmartDuelEventConstants.RoomGetDuelistsAction)), Times.Once);
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
        }

        [TestCase("")]
        [TestCase("     ")]
        [TestCase(null)]
        [Parallelizable()]
        public void Given_InvalidRoomName_When_EnterRoomButtonPressed_Then_RoomNotEntered(string invalidName)
        {
            _viewModel.UpdateRoomName(invalidName);

            _viewModel.OnEnterRoomPressed();

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
        public void Given_InvalidDuelists_When_SpectateButtonPressed_Then_ToastMessageShown(string invalidDuelistId)
        {
            _viewModel.OnSpectateButtonPressed(invalidDuelistId);

            _dialogService.Verify(ds => ds.ShowToast("Duelist name is required"), Times.Once);
        }

        [TestCase("")]
        [TestCase("     ")]
        [TestCase(null)]
        [Parallelizable()]
        public void Given_InvalidDuelists_When_SpectateButtonPressed_Then_SpectateEventNotSent(string invalidDuelistId)
        {
            _viewModel.OnSpectateButtonPressed(invalidDuelistId);

            _smartDuelServer.Verify(sds => sds.EmitEvent(It.IsAny<SmartDuelEvent>()), Times.Never);
        }

        [Test]
        public void When_GoBackButtonPressed_Then_DuelRoomStateIsEnterRoomName()
        {
            var expected = new List<DuelRoomState> { DuelRoomState.Loading, DuelRoomState.EnterRoomName };
            var model = new List<DuelRoomState>();
            _viewModel.UpdateDuelRoomState.Subscribe(e => model.Add(e));

            _viewModel.OnGoBackButtonPressed();

            Assert.AreEqual(expected, model);
        }

        [Test]
        public void When_GoBackButtonPressed_Then_RoomNameSetToNull()
        {
            var model = "";
            _viewModel.UpdateRoomNameField.Subscribe(e => model = e);
            
            _viewModel.OnGoBackButtonPressed();

            Assert.IsNull(model);
        }

        [Test]
        public void When_GoBackButtonPressed_Then_ClearDropDownMenusEventFired()
        {
            bool model = false;
            _viewModel.ClearDropDownMenu.Subscribe(e => model = e);

            _viewModel.OnGoBackButtonPressed();

            Assert.IsTrue(model);
        }

        [Test]
        public void When_GoBackButtonPressed_Then_UpdateDropDownMenusEventIsNotFired()
        {
            bool model = true;
            _viewModel.UpdateDropDownMenu.Subscribe(_ => model = false);

            _viewModel.OnGoBackButtonPressed();

            Assert.IsTrue(model);
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
            var expected = new List<DuelRoomState> { DuelRoomState.Loading, DuelRoomState.Loading };
            var model = new List<DuelRoomState>();
            _viewModel.UpdateDuelRoomState.Subscribe(e => model.Add(e));

            TestUtils.RunAsyncMethodSync(() => _viewModel.OnTryAgainButtonPressed());

            Assert.AreEqual(expected, model);
        }

        [Test]
        public void When_TryAgainButtonPressed_Then_ClearDropDownMenusEventFired()
        {
            bool model = false;
            _viewModel.ClearDropDownMenu.Subscribe(e => model = e);

            TestUtils.RunAsyncMethodSync(() => _viewModel.OnTryAgainButtonPressed());

            Assert.IsTrue(model);
        }

        [Test]
        public void When_TryAgainButtonPressed_Then_UpdateDropDownMenusEventIsNotFired()
        {
            bool model = true;
            _viewModel.UpdateDropDownMenu.Subscribe(_ => model = false);

            TestUtils.RunAsyncMethodSync(() => _viewModel.OnTryAgainButtonPressed());

            Assert.IsTrue(model);
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
            var expected = new List<DuelRoomState> { DuelRoomState.Loading, DuelRoomState.EnterRoomName };
            var model = new List<DuelRoomState>();
            _viewModel.UpdateDuelRoomState.Subscribe(e => model.Add(e));

            _viewModel.OnLeaveRoomButtonPressed();

            Assert.AreEqual(expected, model);
        }

        [Test]
        public void When_LeaveRoomButtonPressed_Then_ClearDropDownMenusEventFired()
        {
            bool model = false;
            _viewModel.ClearDropDownMenu.Subscribe(e => model = e);

            _viewModel.OnLeaveRoomButtonPressed();

            Assert.IsTrue(model);
        }

        [Test]
        public void When_LeaveRoomButtonPressed_Then_UpdateDropDownMenusEventIsNotFired()
        {
            bool model = true;
            _viewModel.UpdateDropDownMenu.Subscribe(_ => model = false);

            _viewModel.OnLeaveRoomButtonPressed();

            Assert.IsTrue(model);
        }

        #endregion

        #region Form Fields Tests

        [Test]
        public void When_UpdateRoomName_Then_UpdateRoomNameFieldEventFired()
        {
            var randomString = _randomizer.GetString(5);
            var model = "";
            _viewModel.UpdateRoomNameField.Subscribe(e => model = e);

            _viewModel.UpdateRoomName(randomString);

            Assert.AreEqual(randomString, model);
        }

        #endregion

        #region Global Event Logic Tests

        [Test]
        public void When_GlobalConnectEvent_Then_EnterRoomNameStatReturned()
        {
            var expected = new List<DuelRoomState> { DuelRoomState.Loading, DuelRoomState.EnterRoomName };
            var model = new List<DuelRoomState>();
            _viewModel.UpdateDuelRoomState.Subscribe(e => model.Add(e));
            var globalConnectEvent = new SmartDuelEvent(
                SmartDuelEventConstants.GlobalScope,
                SmartDuelEventConstants.GlobalConnectAction,
                null);

            _viewModel.OnSmartDuelEventReceived(globalConnectEvent);

            Assert.AreEqual(expected, model);
        }

        [TestCase(SmartDuelEventConstants.GlobalConnectErrorAction)]
        [TestCase(SmartDuelEventConstants.GlobalConnectTimeoutAction)]
        [TestCase(SmartDuelEventConstants.GlobalErrorAction)]
        [Parallelizable()]
        public void When_GlobalErrorEvent_Then_ErrorStateREturned(string error)
        {
            var expected = new List<DuelRoomState> { DuelRoomState.Loading, DuelRoomState.Error };
            var model = new List<DuelRoomState>();
            _viewModel.UpdateDuelRoomState.Subscribe(e => model.Add(e));
            var globalErrorEvent = new SmartDuelEvent(
                SmartDuelEventConstants.GlobalScope,
                error,
                null);

            _viewModel.OnSmartDuelEventReceived(globalErrorEvent);

            Assert.AreEqual(expected, model);
        }

        [TestCase(SmartDuelEventConstants.GlobalConnectErrorAction)]
        [TestCase(SmartDuelEventConstants.GlobalConnectTimeoutAction)]
        [TestCase(SmartDuelEventConstants.GlobalErrorAction)]
        [Parallelizable()]
        public void When_GlobalErrorEvent_Then_ErrorMessageAppears(string expectedError)
        {
            string model = "";
            _viewModel.UpdateErrorTextField.Subscribe(e => model = e);
            var globalErrorEvent = new SmartDuelEvent(
                SmartDuelEventConstants.GlobalScope,
                expectedError,
                null);

            _viewModel.OnSmartDuelEventReceived(globalErrorEvent);

            Assert.AreEqual(expectedError, model);
        }

        #endregion

        #region Room Event Logic Tests

        [TestCase(SmartDuelEventConstants.RoomGetDuelistsAction)]
        [TestCase(SmartDuelEventConstants.RoomLeaveAction)]
        [TestCase(SmartDuelEventConstants.RoomSpectateAction)]
        [TestCase(SmartDuelEventConstants.RoomStartAction)]
        [TestCase(SmartDuelEventConstants.RoomCloseAction)]
        [Parallelizable()]
        public void Given_InvalidRoomEvent_When_Handled_ErrorStateReturned(string eventName)
        {
            var expected = new List<DuelRoomState> { DuelRoomState.Loading, DuelRoomState.Error };
            var model = new List<DuelRoomState>();
            _viewModel.UpdateDuelRoomState.Subscribe(e => model.Add(e));
            CardEventData invalidEventArgs = null;
            var invalidRoomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                eventName,
                invalidEventArgs);

            _viewModel.OnSmartDuelEventReceived(invalidRoomEvent);

            Assert.AreEqual(expected, model);
        }

        [TestCase(SmartDuelEventConstants.RoomGetDuelistsAction)]
        [TestCase(SmartDuelEventConstants.RoomLeaveAction)]
        [TestCase(SmartDuelEventConstants.RoomSpectateAction)]
        [TestCase(SmartDuelEventConstants.RoomStartAction)]
        [TestCase(SmartDuelEventConstants.RoomCloseAction)]
        [Parallelizable()]
        public void Given_InvalidRoomEvent_When_Handled_ErrorTextReturned(string eventName)
        {
            var model = "";
            _viewModel.UpdateErrorTextField.Subscribe(e => model = e);
            CardEventData invalidEventArgs = null;
            var invalidRoomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                eventName,
                invalidEventArgs);

            _viewModel.OnSmartDuelEventReceived(invalidRoomEvent);

            Assert.AreEqual("Room Data is Invalid!", model);
        }

        [Test]
        public void Given_RoomDataWithError_When_HandleGetDuelistsEventCalled_Then_ErrorStateReturned()
        {
            var expected = new List<DuelRoomState> { DuelRoomState.Loading, DuelRoomState.Error };
            var model = new List<DuelRoomState>();
            _viewModel.UpdateDuelRoomState.Subscribe(e => model.Add(e));
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomGetDuelistsAction,
                new RoomEventData { Error = "Error" });

            _viewModel.OnSmartDuelEventReceived(roomEvent);

            Assert.AreEqual(expected, model);
        }

        [Test]
        public void Given_RoomDataWithError_When_HandleGetDuelistEventCalled_ErrorTextReturned()
        {
            var model = "";
            _viewModel.UpdateErrorTextField.Subscribe(e => model = e);
            var invalidRoomEventData = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomGetDuelistsAction,
                new RoomEventData { Error = "Error" });

            _viewModel.OnSmartDuelEventReceived(invalidRoomEventData);

            Assert.AreEqual("Error", model);
        }

        [Test]
        public void Given_ValidDuelistID_When_HandleGetDuelistsEventCalled_Then_ReturnSelectDuelistState()
        {
            var expected = new List<DuelRoomState> { DuelRoomState.Loading, DuelRoomState.SelectDuelist };
            var model = new List<DuelRoomState>();
            _viewModel.UpdateDuelRoomState.Subscribe(e => model.Add(e));
            var data = new RoomEventData { DuelistsIds = new List<string> { "validID", "validID2" } };
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomGetDuelistsAction,
                data);

            _viewModel.OnSmartDuelEventReceived(roomEvent);

            Assert.AreEqual(expected, model);
        }

        [Test]
        public void Given_ValidDuelistID_When_HandleGetDuelistsEventCalled_Then_UpdateDropDownEventFired()
        {
            var expected = new List<string> { "validID", "validID2" };
            var model = new List<string>();
            _viewModel.UpdateDropDownMenu.Subscribe(e => model = e);
            var data = new RoomEventData { DuelistsIds = expected };
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomGetDuelistsAction,
                data);

            _viewModel.OnSmartDuelEventReceived(roomEvent);

            Assert.AreEqual(expected, model);
        }

        [Test]
        public void When_RoomSpectateEventRecieved_Then_WaitingDuelRoomStateIsReturned()
        {
            var expected = new List<DuelRoomState> { DuelRoomState.Loading, DuelRoomState.Waiting };
            var model = new List<DuelRoomState>();
            _viewModel.UpdateDuelRoomState.Subscribe(e => model.Add(e));
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomSpectateAction,
                new RoomEventData());

            _viewModel.OnSmartDuelEventReceived(roomEvent);

            Assert.AreEqual(expected, model);
        }

        [Test]
        public void When_RoomStartEvent_Then_RoomSaved()
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
        public void When_RoomStartEvent_Then_SpeedDuelSceneShown()
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
        public void When_RoomStartEvent_Then_LoadingStateReturned()
        {
            var expected = new List<DuelRoomState> { DuelRoomState.Loading, DuelRoomState.Loading };
            var model = new List<DuelRoomState>();
            _viewModel.UpdateDuelRoomState.Subscribe(e => model.Add(e));
            var room = new RoomEventData { DuelRoom = new Code.Core.SmartDuelServer.Entities.EventData.RoomEvents.DuelRoom() };
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomStartAction,
                room);

            _viewModel.OnSmartDuelEventReceived(roomEvent);

            Assert.AreEqual(expected, model);
        }

        [Test]
        public void Given_NullRoom_When_RoomStartEvent_Then_ErrorStateReturned()
        {
            var expected = new List<DuelRoomState> { DuelRoomState.Loading, DuelRoomState.Error };
            var model = new List<DuelRoomState>();
            _viewModel.UpdateDuelRoomState.Subscribe(e => model.Add(e));
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomStartAction,
                new RoomEventData());

            _viewModel.OnSmartDuelEventReceived(roomEvent);

            Assert.AreEqual(expected, model);
        }

        [Test]
        public void Given_NullRoom_When_RoomStartEvent_Then_ErrorTextReturned()
        {
            var model = "";
            _viewModel.UpdateErrorTextField.Subscribe(e => model = e);
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomStartAction,
                new RoomEventData());

            _viewModel.OnSmartDuelEventReceived(roomEvent);

            Assert.AreEqual("invalid duel room data", model);
        }

        [Test]
        public void Given_NullRoom_When_RoomStartEvent_Then_SceneDoesntChange()
        {
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomStartAction,
                new RoomEventData());

            _viewModel.OnSmartDuelEventReceived(roomEvent);

            _navigationService.Verify(ns => ns.ShowSpeedDuelScene(), Times.Never);
        }

        [Test]
        public void When_RoomClosedEventRecieved_Then_ErrorDuelRoomStateReturned()
        {
            var expected = new List<DuelRoomState> { DuelRoomState.Loading, DuelRoomState.Error };
            var model = new List<DuelRoomState>();
            _viewModel.UpdateDuelRoomState.Subscribe(e => model.Add(e));
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomCloseAction,
                new RoomEventData());

            _viewModel.OnSmartDuelEventReceived(roomEvent);

            Assert.AreEqual(expected, model);
        }

        [Test]
        public void When_RoomClosedEventRecieved_Then_ErrorTextReturned()
        {
            var expected = "roomIsClosed";
            var model = "";
            _viewModel.UpdateErrorTextField.Subscribe(e => model = e);
            var roomEvent = new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomCloseAction,
                new RoomEventData());

            _viewModel.OnSmartDuelEventReceived(roomEvent);

            Assert.AreEqual(expected, model);
        }

        #endregion
    }
}