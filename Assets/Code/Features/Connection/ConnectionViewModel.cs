using Code.Core.DataManager;
using Code.Core.DataManager.Connections.Entities;
using Code.Core.Dialog;
using Code.Core.Logger;
using Code.Core.Navigation;
using Code.Core.Screen;
using Code.Features.Connection.Helpers;
using System;
using UniRx;

namespace Code.Features.Connection
{
    public class ConnectionViewModel
    {
        private const string Tag = "ConnectionViewModel";

        private readonly ConnectionFormValidators _connectionFormValidators;
        private readonly IDataManager _dataManager;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IScreenService _screenService;
        private readonly IAppLogger _logger;

        #region Properties

        private readonly BehaviorSubject<bool> _toggleDeveloperMode = new BehaviorSubject<bool>(false);
        public IObservable<bool> ToggleDeveloperMode => _toggleDeveloperMode;

        private readonly BehaviorSubject<bool> _toggleLocalConnectionMenu = new BehaviorSubject<bool>(false);
        public IObservable<bool> ToggleLocalConnectionMenu => _toggleLocalConnectionMenu;

        private readonly BehaviorSubject<bool> _toggleSettingsMenu = new BehaviorSubject<bool>(false);
        public IObservable<bool> ToggleSettingsMenu => _toggleSettingsMenu;

        private readonly BehaviorSubject<string> _ipAddress = new BehaviorSubject<string>(default);
        public IObservable<string> IpAddress => _ipAddress;

        private readonly BehaviorSubject<string> _port = new BehaviorSubject<string>(default);
        public IObservable<string> Port => _port;

        #endregion

        #region Constructors

        public ConnectionViewModel(
            ConnectionFormValidators connectionFormValidators,
            IDataManager dataManager,
            IDialogService dialogService,
            INavigationService navigationService,
            IScreenService screenService,
            IAppLogger appLogger)
        {
            _connectionFormValidators = connectionFormValidators;

            _dataManager = dataManager;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _screenService = screenService;
            _logger = appLogger;
        }

        #endregion

        #region Initialization

        public void Init()
        {
            _logger.Log(Tag, "Init()");

            _screenService.UsePortraitOrientation();

            InitSettings();
            InitForm();
        }

        private void InitSettings()
        {
            _logger.Log(Tag, "InitSettings()");
            
            var isInDeveloperMode = _dataManager.IsDeveloperModeEnabled();
            _toggleDeveloperMode.OnNext(isInDeveloperMode);
        }
        
        private void InitForm()
        {
            _logger.Log(Tag, "InitForm()");

            var connectionInfo = _dataManager.GetConnectionInfo(forceLocalInfo: true);
            if (connectionInfo == null)
            {
                return;
            }

            _ipAddress.OnNext(connectionInfo.IpAddress);
            _port.OnNext(connectionInfo.Port);
        }

        #endregion

        #region Form fields

        public void OnSettingsMenuToggled(bool state)
        {
            _logger.Log(Tag, $"OnSettingsMenuToggled(State: {state})");

            _toggleSettingsMenu.OnNext(state);
        }

        public void OnDeveloperModeToggled(bool state)
        {
            _logger.Log(Tag, $"OnDeveloperModeToggled(State: {state})");

            _dataManager.SaveDeveloperModeEnabled(state);

            _toggleLocalConnectionMenu.OnNext(state);
        }
        
        public void OnIpAddressChanged(string ipAddress)
        {
            _logger.Log(Tag, $"OnIpAddressSubmitted(ipAddress: {ipAddress})");

            _ipAddress.OnNext(ipAddress);
        }
        
        public void OnPortChanged(string port)
        {
            _logger.Log(Tag, $"OnIpAddressSubmitted(port: {port})");

            _port.OnNext(port);
        }

        #endregion

        #region Button actions

        public void OnEnterOnlineDuelRoomPressed()
        {
            _logger.Log(Tag, "OnEnterOnlineDuelRoomPressed()");

            _dataManager.SaveUseOnlineDuelRoom(true);

            EnterDuelRoom();
        }

        public void OnEnterLocalDuelRoomPressed()
        {
            _logger.Log(Tag, "OnEnterLocalDuelRoomPressed()");

            var isFormValid = ValidateForm();
            if (!isFormValid)
            {
                return;
            }

            var connectionInfo = new ConnectionInfo(_ipAddress.Value, _port.Value);
            _dataManager.SaveConnectionInfo(connectionInfo);

            _dataManager.SaveUseOnlineDuelRoom(false);

            EnterDuelRoom();
        }

        private bool ValidateForm()
        {
            _logger.Log(Tag, "ValidateForm()");

            return ValidateIpAddress(_ipAddress.Value) && ValidatePort(_port.Value);
        }

        private bool ValidateIpAddress(string ipAddress)
        {
            _logger.Log(Tag, $"ValidateIpAddress(ipAddress: {ipAddress})");

            string toastMessage = default;

            if (string.IsNullOrEmpty(ipAddress))
            {
                toastMessage = "IP address is required.";
            }
            else if (!_connectionFormValidators.IsValidIpAddress(ipAddress))
            {
                toastMessage = "Not a valid IP address.";
            }

            if (toastMessage != default)
            {
                _dialogService.ShowToast(toastMessage);
            }

            return toastMessage == default;
        }

        private bool ValidatePort(string port)
        {
            _logger.Log(Tag, $"ValidatePort(port: {port})");

            string toastMessage = default;

            if (string.IsNullOrEmpty(port))
            {
                toastMessage = "Port is required.";
            }
            else if (!_connectionFormValidators.IsValidPort(port))
            {
                toastMessage = "Not a valid port.";
            }

            if (toastMessage != default)
            {
                _dialogService.ShowToast(toastMessage);
            }

            return toastMessage == default;
        }

        private void EnterDuelRoom()
        {
            _logger.Log(Tag, "EnterDuelRoom()");

            _navigationService.ShowDuelRoomScene();
        }

        #endregion

        #region Clean-up

        public void Dispose()
        {
            _logger.Log(Tag, "Dispose()");

            _ipAddress.Dispose();
            _port.Dispose();
        }

        #endregion
    }
}