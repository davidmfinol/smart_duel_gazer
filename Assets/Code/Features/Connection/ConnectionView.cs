using Code.Core.DataManager;
using Code.Core.DataManager.Connections.Entities;
using Code.Core.Dialog;
using Code.Core.Navigation;
using Code.Core.Screen;
using Code.Features.Connection.Helpers;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.Features.Connection
{
    public class ConnectionView : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _ipAddressInputField;

        [SerializeField] private TMP_InputField _portInputField;

        [SerializeField] private Button _speedDuelButton;

        private ConnectionFormValidators _connectionFormValidators;
        private INavigationService _navigationService;
        private IDialogService _dialogService;
        private IDataManager _dataManager;

        #region Constructors

        [Inject]
        public void Construct(
            ConnectionFormValidators connectionFormValidators,
            INavigationService navigationService,
            IDialogService dialogService,
            IScreenService screenService,
            IDataManager dataManager)
        {
            _connectionFormValidators = connectionFormValidators;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _dataManager = dataManager;

            screenService.UsePortraitOrientation();
            InitFormFields();
        }

        #endregion

        #region Connection form

        private void InitFormFields()
        {
            var connectionInfo = _dataManager.GetConnectionInfo();
            if (connectionInfo != null)
            {
                _ipAddressInputField.text = connectionInfo.IpAddress;
                _portInputField.text = connectionInfo.Port;
            }

            _speedDuelButton.onClick.AsObservable().Subscribe(_ => OnConnectButtonPressed());
        }

        private void OnConnectButtonPressed()
        {
            var isFormValid = ValidateForm();
            if (!isFormValid)
            {
                return;
            }

            SaveConnectionInfo();
            ShowDuelRoomScene();
        }

        private bool ValidateForm()
        {
            string toastMessage = default;

            if (string.IsNullOrEmpty(_ipAddressInputField.text))
            {
                toastMessage = "IP address is required.";
            }
            else if (string.IsNullOrEmpty(_portInputField.text))
            {
                toastMessage = "Port is required.";
            }
            else if (!_connectionFormValidators.IsValidIpAddress(_ipAddressInputField.text))
            {
                toastMessage = "Not a valid IP address.";
            }
            else if (!_connectionFormValidators.IsValidPort(_portInputField.text))
            {
                toastMessage = "Not a valid port.";
            }

            if (toastMessage != default)
            {
                _dialogService.ShowToast(toastMessage);
            }

            return toastMessage == default;
        }

        private void SaveConnectionInfo()
        {
            var connectionInfo = new ConnectionInfo(_ipAddressInputField.text, _portInputField.text);
            _dataManager.SaveConnectionInfo(connectionInfo);
        }

        private void ShowDuelRoomScene()
        {
            _navigationService.ShowDuelRoomScene();
        }

        #endregion
    }
}