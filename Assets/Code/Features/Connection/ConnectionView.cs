using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using UniRx;
using AssemblyCSharp.Assets.Code.Features.Connection.Helpers;
using AssemblyCSharp.Assets.Code.Core.Navigation.Interface;
using AssemblyCSharp.Assets.Code.Core.Dialog.Interface;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.Screen.Interface;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Connection.Entities;

namespace AssemblyCSharp.Assets.Code.Features.Connection
{
    public class ConnectionView : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField ipAddressInputField;

        [SerializeField]
        private TMP_InputField portInputField;

        [SerializeField]
        private Button connectButton;

        private ConnectionFormValidators _connectionFormValidators;
        private INavigationService _navigationService;
        private IDialogService _dialogService;
        private IDataManager _dataManager;

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

        private void InitFormFields()
        {
            var connectionInfo = _dataManager.GetConnectionInfo();
            if (connectionInfo != null)
            {
                ipAddressInputField.text = connectionInfo.IpAddress;
                portInputField.text = connectionInfo.Port;
            }

            connectButton.onClick.AsObservable().Subscribe(_ => OnConnectPressed());
        }

        public void OnConnectPressed()
        {
            var isFormValid = ValidateForm();
            if (!isFormValid)
            {
                return;
            }

            SaveConnectionInfo();
            ShowMainScene();
        }

        private bool ValidateForm()
        {
            string toastMessage = default;

            if (string.IsNullOrEmpty(ipAddressInputField.text))
            {
                toastMessage = "IP address is required.";
            }
            else if (string.IsNullOrEmpty(portInputField.text))
            {
                toastMessage = "Port is required.";
            }
            else if (!_connectionFormValidators.IsValidIpAddress(ipAddressInputField.text))
            {
                toastMessage = "Not a valid IP address.";
            }
            else if (!_connectionFormValidators.IsValidPort(portInputField.text))
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
            var connectionInfo = new ConnectionInfo(ipAddressInputField.text, portInputField.text);
            _dataManager.SaveConnectionInfo(connectionInfo);
        }

        private void ShowMainScene()
        {
            _navigationService.ShowMainScene();
        }
    }
}
