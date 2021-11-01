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
using Castle.Core.Internal;
using Code.Core.Localization;
using Code.Core.Localization.Entities;

namespace Code.Features.DuelRoom
{
    public class DuelRoomViewModel
    {
        private const string Tag = "DuelRoomViewModel";

        private const int CloseSocketDuration = 200;

        private readonly IDataManager _dataManager;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly ISmartDuelServer _smartDuelServer;
        private readonly IDelayProvider _delayProvider;
        private readonly IStringProvider _stringProvider;
        private readonly IScreenService _screenService;
        private readonly IAppLogger _logger;

        private IDisposable _smartDuelEventSubscription;

        private string _duelistToSpectate;
        private bool _startedDuelSuccessfully;

        #region Properties
        
        private readonly BehaviorSubject<DuelRoomState> _roomState = new BehaviorSubject<DuelRoomState>(DuelRoomState.Loading);
        public IObservable<DuelRoomState> RoomState => _roomState;

        private readonly BehaviorSubject<string> _roomName = new BehaviorSubject<string>(default);
        public IObservable<string> RoomName => _roomName;

        private readonly BehaviorSubject<string> _errorText = new BehaviorSubject<string>(default);
        public IObservable<string> ErrorText => _errorText;

        private readonly BehaviorSubject<List<string>> _duelistIds = new BehaviorSubject<List<string>>(default);
        public IObservable<List<string>> DuelistIds => _duelistIds;

        #endregion

        #region Constructor

        public DuelRoomViewModel(
            IDataManager dataManager,
            INavigationService navigationService,
            IDialogService dialogService,
            ISmartDuelServer smartDuelServer,
            IDelayProvider delayProvider,
            IStringProvider stringProvider,
            IScreenService screenService,
            IAppLogger appLogger)
        {
            _dataManager = dataManager;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _smartDuelServer = smartDuelServer;
            _delayProvider = delayProvider;
            _stringProvider = stringProvider;
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
            _logger.Log(Tag, $"InitSmartDuelEventSubscription()");
            
            _smartDuelEventSubscription = _smartDuelServer.GlobalEvents
                .Merge(_smartDuelServer.RoomEvents)
                .Subscribe(OnSmartDuelEventReceived);

            _smartDuelServer.Init();
        }

        #endregion

        #region Button Events

        public void OnEnterRoomPressed()
        {
            _logger.Log(Tag, "OnEnterRoomPressed()");

            if (string.IsNullOrWhiteSpace(_roomName.Value))
            {
                var message = _stringProvider.GetString(LocaleKeys.DuelRoomRoomNameRequired);
                _dialogService.ShowToast(message);
                return;
            }

            SendGetDuelistsInRoomEvent();
        }

        public void OnSpectateButtonPressed(string duelist)
        {
            _logger.Log(Tag, $"OnSpectateButtonPressed(duelist: {duelist})");
            
            if (string.IsNullOrWhiteSpace(duelist))
            {
                var message = _stringProvider.GetString(LocaleKeys.DuelRoomDuelistNameRequired);
                _dialogService.ShowToast(message);
                return;
            }

            _duelistToSpectate = duelist;

            SendSpectateRoomEvent();
        }

        public void OnGoBackButtonPressed()
        {
            _logger.Log(Tag, "OnGoBackButtonPressed()");
            
            ResetFields();
            SendLeaveRoomEvent();
            
            _roomState.OnNext(DuelRoomState.EnterRoomName);
        }

        public async Task OnTryAgainButtonPressed()
        {
            _logger.Log(Tag, "OnTryAgainButtonPressed()");
            
            ResetFields();

            _roomState.OnNext(DuelRoomState.Loading);

            _smartDuelServer.Dispose();
            await _delayProvider.Wait(CloseSocketDuration);
            _smartDuelServer.Init();
        }

        public void OnLeaveRoomButtonPressed()
        {
            _logger.Log(Tag, "OnLeaveRoomButtonPressed()");
            
            ResetFields();
            SendLeaveRoomEvent();

            _roomState.OnNext(DuelRoomState.EnterRoomName);
        }

        private void ResetFields()
        {
            _logger.Log(Tag, "ResetFields()");
            
            _duelistToSpectate = null;

            UpdateDuelistsToSpectateList(null);
            _roomName.OnNext(null);
        }

        #endregion

        #region Form Fields

        public void UpdateRoomName(string name)
        {
            _logger.Log(Tag, $"UpdateRoomName(name: {name})");
            
            _roomName.OnNext(name);
        }

        private void UpdateDuelistsToSpectateList(RoomEventData data)
        {
            _logger.Log(Tag, $"UpdateDuelistsToSpectateList(data: {data})");
            
            if (data == null || data.DuelistsIds.IsNullOrEmpty())
            {
                _duelistIds.OnNext(null);
                return;
            }

            var currentDuelistIds = _duelistIds.Value;
            if (!currentDuelistIds.IsNullOrEmpty())
            {
                var message = _stringProvider.GetString(LocaleKeys.DuelRoomDuelistAppeared);
                _dialogService.ShowToast(message);
            }

            var duelistIds = data.DuelistsIds!.ToList();
            _duelistIds.OnNext(duelistIds);
        }

        #endregion

        #region Send Smart Duel Events

        private void SendGetDuelistsInRoomEvent()
        {
            _logger.Log(Tag, "SendGetDuelistsInRoomEvent()");
            
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
            _logger.Log(Tag, "SendSpectateRoomEvent()");
            
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
            _logger.Log(Tag, "SendLeaveRoomEvent()");
            
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

        #region Receive Global Event

        private void HandleGlobalEvent(SmartDuelEvent e)
        {
            _logger.Log(Tag, $"HandleGlobalEvent(scope: {e.Scope}, action: {e.Action})");
            
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
            _logger.Log(Tag, "HandleConnectEvent()");
            
            _roomState.OnNext(DuelRoomState.EnterRoomName);
        }

        private void HandleErrorEvent(string error)
        {
            _logger.Log(Tag, $"HandleErrorEvent(error: {error})");

            var fullErrorText = _stringProvider.GetString(LocaleKeys.DuelistRoomConnectionErrorPrefix, error);

            _roomState.OnNext(DuelRoomState.Error);
            _errorText.OnNext(fullErrorText);
        }

        #endregion

        #region Receive Room Events
        
        private void HandleRoomEvent(SmartDuelEvent e)
        {
            _logger.Log(Tag, $"HandleRoomEvent(e: {e})");
            
            if (!(e.Data is RoomEventData data))
            {
                var error = _stringProvider.GetString(LocaleKeys.DuelistRoomConnectionErrorDataInvalid);
                HandleErrorEvent(error);
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
            _logger.Log(Tag, $"HandleRoomGetDuelistsEvent(data: {data})");
            
            if (!data.Error.IsNullOrEmpty())
            {
                HandleErrorEvent(data.Error);
                return;
            }

            UpdateDuelistsToSpectateList(data);
            _roomState.OnNext(DuelRoomState.SelectDuelist);
        }

        private void HandleRoomSpectateEvent()
        {
            _logger.Log(Tag, "HandleRoomSpectateEvent()");
            
            _roomState.OnNext(DuelRoomState.Waiting);
        }

        private void HandleRoomStartEvent(RoomEventData data)
        {
            _logger.Log(Tag, $"HandleRoomStartEvent(data: {data})");
            
            var duelRoom = data.DuelRoom;
            if (duelRoom == null)
            {
                var error = _stringProvider.GetString(LocaleKeys.DuelistRoomConnectionErrorNoData);
                HandleErrorEvent(error);
                return;
            }

            duelRoom.DuelistToSpectate = _duelistToSpectate;
            _dataManager.SaveDuelRoom(duelRoom);

            _startedDuelSuccessfully = true;
            _navigationService.ShowSpeedDuelScene();
            _roomState.OnNext(DuelRoomState.Loading);
        }

        private void HandleRoomCloseEvent()
        {
            _logger.Log(Tag, "HandleRoomSpectateEvent()");

            var error = _stringProvider.GetString(LocaleKeys.DuelistRoomConnectionErrorRoomClosed);
            HandleErrorEvent(error);
        }

        #endregion

        #endregion

        #region Clean-up

        public void Dispose()
        {
            _logger.Log(Tag, "Dispose()");
            
            _roomName?.Dispose();
            _roomState?.Dispose();
            _errorText?.Dispose();
            _duelistIds?.Dispose();

            _smartDuelEventSubscription?.Dispose();
            _smartDuelEventSubscription = null;

            // Don't dispose server if the duel screen is shown.
            if (!_startedDuelSuccessfully)
            {
                _smartDuelServer?.Dispose();
            }
        }

        #endregion
    }
}