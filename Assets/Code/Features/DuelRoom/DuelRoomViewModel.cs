using Code.Core.SmartDuelServer.Entities.EventData.RoomEvents;
using Code.Core.SmartDuelServer.Entities;
using Code.Features.DuelRoom.Models;
using System.Threading.Tasks;
using Code.Core.Config.Providers;
using Code.Core.DataManager;
using Code.Core.Dialog;
using Code.Core.Navigation;
using Code.Core.SmartDuelServer;
using WebSocketSharp;
using UniRx;
using System;
using Code.Core.Screen;
using Code.Core.Logger;

namespace Code.Features.DuelRoom
{
    public class DuelRoomViewModel
    {
        private const string Tag = "DuelRoomViewModel";

        private readonly IDataManager _dataManager;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly ISmartDuelServer _smartDuelServer;
        private readonly IDelayProvider _delayProvider;
        private readonly IScreenService _screenService;
        private readonly IAppLogger _logger;

        private string _duelistToSpectate;
        private bool _startedDuelSuccessfully;
        private int _waitForSocketToClose = 200;

        private readonly BehaviorSubject<string> _roomName = new BehaviorSubject<string>(default);
        private readonly BehaviorSubject<DuelRoomState> _duelRoomState = new BehaviorSubject<DuelRoomState>(default);
        private readonly BehaviorSubject<string> _errorText = new BehaviorSubject<string>(default);
        private readonly BehaviorSubject<RoomEventData> _dropdownData = new BehaviorSubject<RoomEventData>(default);

        #region Observer Accessors

        public IObservable<string> UpdateRoomNameField => _roomName;
        public IObservable<DuelRoomState> UpdateDuelRoomState => _duelRoomState;
        public IObservable<string> UpdateErrorTextField => _errorText;
        public IObservable<RoomEventData> UpdateDropDownMenu => _dropdownData;

        #endregion

        #region Constructor

        public DuelRoomViewModel(
            IDataManager dataManager,
            INavigationService navigationService,
            IDialogService dialogService,
            ISmartDuelServer smartDuelServer,
            IDelayProvider delayProvider,
            IScreenService screenService,
            IAppLogger appLogger)
        {
            _dataManager = dataManager;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _smartDuelServer = smartDuelServer;
            _delayProvider = delayProvider;
            _screenService = screenService;
            _logger = appLogger;

            Init();
        }

        #endregion

        private void Init()
        {
            _logger.Log(Tag, $"Init()");

            _screenService.UsePortraitOrientation();
        }

        #region Button Events

        public void OnEnterRoomPressed()
        {
            _logger.Log(Tag, "OnEnterRoomPressed()");
            
            if (string.IsNullOrWhiteSpace(_roomName.Value))
            {
                _dialogService.ShowToast("Room name is required");
                return;
            }

            SendGetDuelistsInRoomEvent();
        }

        public void OnSpectateButtonPressed(string duelist)
        {
            if (string.IsNullOrWhiteSpace(duelist))
            {
                _dialogService.ShowToast("Duelist name is required");
                return;
            }

            _duelistToSpectate = duelist;

            SendSpectateRoomEvent();
        }

        public void OnGoBackButtonPressed()
        {
            ResetFields();
            _duelRoomState.OnNext(DuelRoomState.EnterRoomName);
        }

        public async Task OnTryAgainButtonPressed()
        {
            ResetFields();

            _duelRoomState.OnNext(DuelRoomState.Loading);

            _smartDuelServer.Dispose();
            await _delayProvider.Wait(_waitForSocketToClose);
            _smartDuelServer.Init();
        }

        public void OnLeaveRoomButtonPressed()
        {
            ResetFields();
            SendLeaveRoomEvent();

            _duelRoomState.OnNext(DuelRoomState.EnterRoomName);
        }

        private void ResetFields()
        {
            _duelistToSpectate = null;

            _roomName.OnNext(null);
            _dropdownData.OnNext(null);
        }

        #endregion

        #region Form Fields

        public void UpdateRoomName(string name)
        {
            _roomName.OnNext(name);
        }

        public void UpdateDuelistToSpectate(string duelist)
        {
            _duelistToSpectate = duelist;
        }

        #endregion

        #region Send smart duel events

        private void SendGetDuelistsInRoomEvent()
        {
            _smartDuelServer.EmitEvent(new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomGetDuelistsAction,
                new RoomEventData
                {
                    RoomName = _roomName.Value
                }));
        }

        private void SendSpectateRoomEvent()
        {
            _smartDuelServer.EmitEvent(new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomSpectateAction,
                new RoomEventData
                {
                    RoomName = _roomName.Value
                }));
        }

        private void SendLeaveRoomEvent()
        {
            _smartDuelServer.EmitEvent(new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomLeaveAction));
        }

        #endregion

        #region Recieve Smart Duel Events

        public void OnSmartDuelEventReceived(SmartDuelEvent e)
        {
            _logger.Log(Tag, $"OnSmartDuelEventReceived(scope: {e.Scope}, action: {e.Action})");

            switch (e.Scope)
            {
                case SmartDuelEventConstants.GlobalScope:
                    HandleGlobalEvent(e);
                    break;

                case SmartDuelEventConstants.RoomScope:
                    HandleRoomEvent(e);
                    break;
            }
        }

        #region Receive global event

        private void HandleGlobalEvent(SmartDuelEvent e)
        {
            switch (e.Action)
            {
                case SmartDuelEventConstants.GlobalConnectAction:
                    HandleConnectEvent();
                    break;

                case SmartDuelEventConstants.GlobalConnectErrorAction:
                case SmartDuelEventConstants.GlobalConnectTimeoutAction:
                case SmartDuelEventConstants.GlobalErrorAction:
                    HandleErrorEvent(e.Action);
                    break;
            }
        }

        private void HandleConnectEvent()
        {
            _duelRoomState.OnNext(DuelRoomState.EnterRoomName);
        }

        private void HandleErrorEvent(string error)
        {
            _logger.Log(Tag, $"Handling Error: {error}");

            _duelRoomState.OnNext(DuelRoomState.Error);
            _errorText.OnNext(error);
        }

        #endregion

        #region Recieve Room Events

        // TODO: Update dropdown when new duelist enters the room
        private void HandleRoomEvent(SmartDuelEvent e)
        {
            if (!(e.Data is RoomEventData data))
            {
                HandleErrorEvent("Room Data is Invalid!");
                return;
            }

            switch (e.Action)
            {
                case SmartDuelEventConstants.RoomGetDuelistsAction:
                    HandleRoomGetDuelistsEvent(data);
                    break;

                case SmartDuelEventConstants.RoomSpectateAction:
                    HandleRoomSpectateEvent();
                    break;

                case SmartDuelEventConstants.RoomStartAction:
                    HandleRoomStartEvent(data);
                    break;

                case SmartDuelEventConstants.RoomCloseAction:
                    HandleRoomCloseEvent();
                    break;
            }
        }

        private void HandleRoomGetDuelistsEvent(RoomEventData data)
        {
            if (!data.Error.IsNullOrEmpty())
            {
                HandleErrorEvent(data.Error);
                return;
            }

            _dropdownData.OnNext(data);
            _duelRoomState.OnNext(DuelRoomState.SelectDuelist);
        }

        private void HandleRoomSpectateEvent()
        {
            _duelRoomState.OnNext(DuelRoomState.Waiting);
        }

        private void HandleRoomStartEvent(RoomEventData data)
        {
            var duelRoom = data.DuelRoom;
            if (duelRoom == null)
            {
                HandleErrorEvent("invalid duel room data");
                return;
            }

            duelRoom.DuelistToSpectate = _duelistToSpectate;
            _dataManager.SaveDuelRoom(duelRoom);

            _startedDuelSuccessfully = true;
            _navigationService.ShowSpeedDuelScene();
            _duelRoomState.OnNext(DuelRoomState.Loading);
        }

        private void HandleRoomCloseEvent()
        {
            HandleErrorEvent("roomIsClosed");
        }

        #endregion

        #endregion

        #region Clean Up

        public void Dispose()
        {
            _roomName.Dispose();
            _duelRoomState.Dispose();
            _errorText.Dispose();
            _dropdownData.Dispose();

            if (_startedDuelSuccessfully) return;

            _smartDuelServer?.Dispose();
        }

        #endregion
    }
}