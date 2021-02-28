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
using Dpoch.SocketIO;

namespace AssemblyCSharp.Assets.Code.Features.Connection
{
    public class ConnectionView : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField _ipAddressInputField;

        [SerializeField]
        private TMP_InputField _portInputField;

        [SerializeField]
        private Button _connectButton;

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
                _ipAddressInputField.text = connectionInfo.IpAddress;
                _portInputField.text = connectionInfo.Port;
            }

            _connectButton.onClick.AsObservable().Subscribe(_ => OnConnectPressed());
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

        private void ShowMainScene()
        {
            _navigationService.ShowMainScene();
        }

        //private const string SUMMON_EVENT_NAME = "summonEvent";
        //private const string REMOVE_CARD_EVENT = "removeCardEvent";

        //private void ConnectToServer()
        //{
        //    var connectionInfo = _dataManager.GetConnectionInfo();

        //    var url = $"ws://{connectionInfo?.IpAddress}:{connectionInfo?.Port}/socket.io/?EIO=4&transport=websocket";
        //    var socket = new SocketIO(url);

        //    socket.OnOpen += () => Debug.Log("Socket open!");
        //    socket.OnConnectFailed += () => Debug.Log("Socket failed to connect!");
        //    socket.OnClose += () => Debug.Log("Socket closed!");
        //    socket.OnError += (err) => Debug.Log("Socket Error: " + err);
        //    socket.On(SUMMON_EVENT_NAME, OnSummonEventReceived);
        //    socket.On(REMOVE_CARD_EVENT, OnRemovecardEventReceived);

        //    socket.Connect();
        //}

        //private void OnSummonEventReceived(SocketIOEvent e)
        //{
        //    print("OnSummonEventReceived");
        //}

        //private void OnRemovecardEventReceived(SocketIOEvent e)
        //{
        //    print("OnRemovecardEventReceived");
        //}
    }
}
