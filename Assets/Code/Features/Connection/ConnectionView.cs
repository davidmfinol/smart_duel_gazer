using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.Features.Connection
{
    public class ConnectionView : MonoBehaviour
    {
        [SerializeField] private Toggle showSettingsMenuToggle;
        [SerializeField] private GameObject settingsMenu;
        [SerializeField] private Toggle developerModeToggle;

        [SerializeField] private GameObject localConnectionForm;
        
        [SerializeField] private Button enterOnlineDuelRoomButton;
        [SerializeField] private TMP_InputField ipAddressInputField;
        [SerializeField] private TMP_InputField portInputField;
        [SerializeField] private Button enterLocalDuelRoomButton;

        private ConnectionViewModel _connectionViewModel;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        #region Lifecycle

        [Inject]
        public void Construct(
            ConnectionViewModel connectionViewModel)
        {
            _connectionViewModel = connectionViewModel;

            OnViewModelSet();
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            
            _connectionViewModel?.Dispose();
        }

        #endregion

        private void OnViewModelSet()
        {
            _connectionViewModel.Init();
            BindViews();
        }

        private void BindViews()
        {
            // Toggles
            _disposables.Add(showSettingsMenuToggle.OnValueChangedAsObservable()
                .Subscribe(_connectionViewModel.OnSettingsMenuToggled));
            _disposables.Add(developerModeToggle.OnValueChangedAsObservable()
                .Subscribe(_connectionViewModel.OnDeveloperModeToggled));

            // Buttons            
            _disposables.Add(enterOnlineDuelRoomButton.onClick.AsObservable()
                .Subscribe(_ => _connectionViewModel.OnEnterOnlineDuelRoomPressed()));
            _disposables.Add(enterLocalDuelRoomButton.onClick.AsObservable()
                .Subscribe(_ => _connectionViewModel.OnEnterLocalDuelRoomPressed()));

            // Input fields
            _disposables.Add(ipAddressInputField.onValueChanged.AsObservable()
                .Subscribe(text => _connectionViewModel.OnIpAddressChanged(text)));
            _disposables.Add(portInputField.onValueChanged.AsObservable()
                .Subscribe(text => _connectionViewModel.OnPortChanged(text)));

            // VM streams
            _disposables.Add(_connectionViewModel.ShowSettingsMenu
                .Subscribe(state => UpdateMenuVisibility(settingsMenu, state)));
            _disposables.Add(_connectionViewModel.IsDeveloperModeEnabled
                .Subscribe(state => UpdateToggleState(developerModeToggle, state)));
            _disposables.Add(_connectionViewModel.ShowLocalConnectionMenu
                .Subscribe(state => UpdateMenuVisibility(localConnectionForm, state)));
            _disposables.Add(_connectionViewModel.IpAddress
                .Subscribe(ipAddress => UpdateInputFieldTextIfNecessary(ipAddressInputField, ipAddress)));
            _disposables.Add(_connectionViewModel.Port
                .Subscribe(port => UpdateInputFieldTextIfNecessary(portInputField, port)));
        }

        private static void UpdateInputFieldTextIfNecessary(TMP_InputField inputField, string text)
        {
            if (!inputField.text.Equals(text))
            {
                inputField.text = text;
            }
        }

        private void UpdateMenuVisibility(GameObject menu, bool isVisible)
        {
            menu.SetActive(isVisible);
        }

        private void UpdateToggleState(Toggle toggle, bool isOn)
        {
            toggle.isOn = isOn;
        }
    }
}