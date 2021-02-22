using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using Zenject;
using AssemblyCSharp.Assets.Code.Core.Navigation.Interface;
using AssemblyCSharp.Assets.Code.Core.Dialog.Interface;
using AssemblyCSharp.Assets.Code.Core.Screen.Interface;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Connection.Entities;

namespace Project.Features.Connection
{
    public class ConnectionView : MonoBehaviour
    {
        private static readonly Regex IP_ADDRESS_REGEX = new Regex(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$");
        private static readonly Regex PORT_REGEX = new Regex("[0-9]+");

        [SerializeField]
        TMP_InputField IpAddressInputField;

        [SerializeField]
        TMP_InputField PortInputField;

        [SerializeField]
        Button ConnectButton;

        private INavigationService _navigationService;
        private IDialogService _dialogService;
        private IDataManager _dataManager;

        [Inject]
        public void Construct(
            INavigationService navigationService,
            IDialogService dialogService,
            IScreenService screenService,
            IDataManager dataManager)
        {
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
                IpAddressInputField.text = connectionInfo.IpAddress;
                PortInputField.text = connectionInfo.Port;
            }

            ConnectButton.onClick.AddListener(OnConnectPressed);
        }

        private void OnConnectPressed()
        {
            var isFormValid = ValidateForm();
            if (!isFormValid)
            {
                return;
            }

            SaveConnectionInfo();
            ShowMainScene();
        }

        // TODO: Create validator class
        private bool ValidateForm()
        {
            string toastMessage = default;

            if (string.IsNullOrEmpty(IpAddressInputField.text))
            {
                toastMessage = "IP address is required.";
            }
            else if (string.IsNullOrEmpty(PortInputField.text))
            {
                toastMessage = "Port is required.";
            }
            else if (!IP_ADDRESS_REGEX.IsMatch(IpAddressInputField.text))
            {
                toastMessage = "Not a valid IP address.";
            }
            else if (!PORT_REGEX.IsMatch(PortInputField.text))
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
            var connectionInfo = new ConnectionInfo(IpAddressInputField.text, PortInputField.text);
            _dataManager.SaveConnectionInfo(connectionInfo);
        }

        private void ShowMainScene()
        {
            _navigationService.ShowMainScene();
        }
    }
}
