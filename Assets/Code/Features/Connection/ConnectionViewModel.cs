using Code.Core.DataManager;
using Code.Core.DataManager.Connections.Entities;
using Code.Core.Dialog;
using Code.Core.Logger;
using Code.Core.Navigation;
using Code.Core.Screen;
using Code.Features.Connection.Helpers;
using System;
using Code.Core.Localization;
using Code.Core.Localization.Entities;
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
        private readonly IStringProvider _stringProvider;
        private readonly IAppLogger _logger;

        #region Properties

        private readonly BehaviorSubject<bool> _developerModeEnabled = new BehaviorSubject<bool>(false);
        public IObservable<bool> IsDeveloperModeEnabled => _developerModeEnabled;

        private readonly BehaviorSubject<bool> _showLocalConnectionMenu = new BehaviorSubject<bool>(false);
        public IObservable<bool> ShowLocalConnectionMenu => _showLocalConnectionMenu;

        private readonly BehaviorSubject<bool> _showSettingsMenu = new BehaviorSubject<bool>(false);
        public IObservable<bool> ShowSettingsMenu => _showSettingsMenu;

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
            IStringProvider stringProvider,
            IAppLogger appLogger)
        {
            _connectionFormValidators = connectionFormValidators;

            _dataManager = dataManager;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _screenService = screenService;
            _stringProvider = stringProvider;
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
            _developerModeEnabled.OnNext(isInDeveloperMode);
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

        public void OnSettingsMenuToggled(bool showSettingsMenu)
        {
            _logger.Log(Tag, $"OnSettingsMenuToggled(showSettingsMenu: {showSettingsMenu})");

            _showSettingsMenu.OnNext(showSettingsMenu);
        }

        public void OnDeveloperModeToggled(bool developerModeEnabled)
        {
            _logger.Log(Tag, $"OnDeveloperModeToggled(developerModeEnabled: {developerModeEnabled})");

            _dataManager.SaveDeveloperModeEnabled(developerModeEnabled);

            _showLocalConnectionMenu.OnNext(developerModeEnabled);
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
                toastMessage = _stringProvider.GetString(LocaleKeys.ConnectionIPAddressRequired);
            }
            else if (!_connectionFormValidators.IsValidIpAddress(ipAddress))
            {
                toastMessage = _stringProvider.GetString(LocaleKeys.ConnectionIPAddressInvalid);
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
                toastMessage = _stringProvider.GetString(LocaleKeys.ConnectionPortRequired);
            }
            else if (!_connectionFormValidators.IsValidPort(port))
            {
                toastMessage = _stringProvider.GetString(LocaleKeys.ConnectionPortInvalid);
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