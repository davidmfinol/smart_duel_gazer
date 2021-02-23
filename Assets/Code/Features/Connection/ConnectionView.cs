using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using UniRx;

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

        private ConnectionViewModel _connectionViewModel;

        [Inject]
        public void Construct(
            ConnectionViewModel connectionViewModel)
        {
            _connectionViewModel = connectionViewModel;

            InitFormFields();
        }

        private void InitFormFields()
        {
            //ipAddressInputField.text = connectionInfo.IpAddress;

            //portInputField.text = connectionInfo.Port;

            connectButton.onClick
                .AsObservable()
                .Subscribe(_ => _connectionViewModel.OnConnectPressed());
        }
    }
}
