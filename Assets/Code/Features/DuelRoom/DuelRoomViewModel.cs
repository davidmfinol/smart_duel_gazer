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
using System.Linq;
using System.Collections.Generic;

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

        private IDisposable _smartDuelEventSubscription;

        private string _duelistToSpectate;
        private bool _startedDuelSuccessfully;
        private int _waitForSocketToClose = 200; 

        #region Properties

        private readonly BehaviorSubject<string> _updateRoomName = new BehaviorSubject<string>(default);
        public IObservable<string> UpdateRoomNameField => _updateRoomName;

        private readonly BehaviorSubject<DuelRoomState> _unpdateDuelRoomState = new BehaviorSubject<DuelRoomState>(default);
        public IObservable<DuelRoomState> UpdateDuelRoomState => _unpdateDuelRoomState;

        private readonly BehaviorSubject<string> _updateErrorTextField = new BehaviorSubject<string>(default);
        public IObservable<string> UpdateErrorTextField => _updateErrorTextField;

        private readonly Subject<bool> _clearDropDownMenu = new Subject<bool>();
        public IObservable<bool> ClearDropDownMenu => _clearDropDownMenu;

        private readonly Subject<List<string>> _updateDropdownDataMenu = new Subject<List<string>>();
        public IObservable<List<string>> UpdateDropDownMenu => _updateDropdownDataMenu;

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
        }

        #endregion

        #region Initialization

        public void Init()
        {
            _logger.Log(Tag, $"Init()");

            InitSmartDuelEventSubscription();
            _screenService.UsePortraitOrientation();
        }

        private void InitSmartDuelEventSubscription()
        {
            _smartDuelEventSubscription = _smartDuelServer.GlobalEvents
                .Merge(_smartDuelServer.RoomEvents)
                .Subscribe(e => OnSmartDuelEventReceived(e));

            _smartDuelServer.Init();
        }

        #endregion

        #region Button Events

        public void OnEnterRoomPressed()
        {
            _logger.Log(Tag, "OnEnterRoomPressed()");
            
            if (string.IsNullOrWhiteSpace(_updateRoomName.Value))
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
            _unpdateDuelRoomState.OnNext(DuelRoomState.EnterRoomName);
        }

        public async Task OnTryAgainButtonPressed()
        {
            ResetFields();

            _unpdateDuelRoomState.OnNext(DuelRoomState.Loading);

            _smartDuelServer.Dispose();
            await _delayProvider.Wait(_waitForSocketToClose);
            _smartDuelServer.Init();
        }

        public void OnLeaveRoomButtonPressed()
        {
            ResetFields();
            SendLeaveRoomEvent();

            _unpdateDuelRoomState.OnNext(DuelRoomState.EnterRoomName);
        }

        private void ResetFields()
        {
            _duelistToSpectate = null;

            UpdateDropDownOptions(null);
            _updateRoomName.OnNext(null);
        }

        #endregion

        #region Form Fields

        public void UpdateRoomName(string name)
        {
            _updateRoomName.OnNext(name);
        }

        public void UpdateDuelistToSpectate(string duelist)
        {
            _duelistToSpectate = duelist;
        }

        private void UpdateDropDownOptions(RoomEventData data)
        {
            if(data == null)
            {
                _clearDropDownMenu.OnNext(true);
                return;
            }
            
            var options = data.DuelistsIds.ToList();
            _clearDropDownMenu.OnNext(true);
            _updateDropdownDataMenu.OnNext(options);
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
                    RoomName = _updateRoomName.Value
                }));
        }

        private void SendSpectateRoomEvent()
        {
            _smartDuelServer.EmitEvent(new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomSpectateAction,
                new RoomEventData
                {
                    RoomName = _updateRoomName.Value
                }));
        }

        private void SendLeaveRoomEvent()
        {
            _smartDuelServer.EmitEvent(new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomLeaveAction));
        }

        #endregion

        #region Receive Smart Duel Events

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
            _unpdateDuelRoomState.OnNext(DuelRoomState.EnterRoomName);
        }

        private void HandleErrorEvent(string error)
        {
            _logger.Log(Tag, $"Handling Error: {error}");

            _unpdateDuelRoomState.OnNext(DuelRoomState.Error);
            _updateErrorTextField.OnNext(error);
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

            UpdateDropDownOptions(data);
            _unpdateDuelRoomState.OnNext(DuelRoomState.SelectDuelist);
        }

        private void HandleRoomSpectateEvent()
        {
            _unpdateDuelRoomState.OnNext(DuelRoomState.Waiting);
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
            _unpdateDuelRoomState.OnNext(DuelRoomState.Loading);
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
            _updateRoomName.Dispose();
            _unpdateDuelRoomState.Dispose();
            _updateErrorTextField.Dispose();
            _updateDropdownDataMenu.Dispose();

            _smartDuelEventSubscription?.Dispose();
            _smartDuelEventSubscription = null;

            if (_startedDuelSuccessfully) return;
            
            _smartDuelServer?.Dispose();
        }

        #endregion
    }
}