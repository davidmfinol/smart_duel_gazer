using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Project.Core.Dialog.Interface;
using Zenject;

namespace Project.Features.Connection
{
    public class ConnectionView : MonoBehaviour
    {
        private static readonly Regex IP_ADDRESS_REGEX = new Regex(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$");
        private static readonly Regex PORT_REGEX = new Regex("[0-9]+");

        private const string CONNECTION_INFO_KEY = "connectionInfo";

        [SerializeField]
        TMP_InputField IpAddressInputField;

        [SerializeField]
        TMP_InputField PortInputField;

        [SerializeField]
        Button ConnectButton;

        //private IDialogService _dialogService;

        //[Inject]
        //public void Construct(IDialogService dialogService)
        //{
        //    _dialogService = dialogService;
        //}

        private void Start()
        {
            SetScreenRotationToPortrait();
            InitFormFields();
        }

        // TODO: create a service for updating the screen rotation.
        private void SetScreenRotationToPortrait()
        {
            Screen.orientation = ScreenOrientation.Portrait;
        }

        private void InitFormFields()
        {
            if (PlayerPrefs.HasKey(CONNECTION_INFO_KEY))
            {
                var json = PlayerPrefs.GetString(CONNECTION_INFO_KEY);
                var connectionInfo = JsonConvert.DeserializeObject<ConnectionInfo>(json);

                IpAddressInputField.text = connectionInfo?.IpAddress;
                PortInputField.text = connectionInfo?.Port;
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
                //_dialogService.ShowToast(toastMessage);
            }

            return toastMessage == default;
        }

        // TODO: create a storage service.
        private void SaveConnectionInfo()
        {
            var connectionInfo = new ConnectionInfo(IpAddressInputField.text, PortInputField.text);
            var json = JsonConvert.SerializeObject(connectionInfo);
            PlayerPrefs.SetString(CONNECTION_INFO_KEY, json);
        }

        // TODO: create a routing service.
        private void ShowMainScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
