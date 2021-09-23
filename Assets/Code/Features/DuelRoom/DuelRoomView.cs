using System;
using Code.Core.Logger;
using Code.Core.SmartDuelServer;
using Code.Core.SmartDuelServer.Entities.EventData.RoomEvents;
using Code.Features.DuelRoom.Helpers;
using Code.Features.DuelRoom.Models;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.Features.DuelRoom
{
    public class DuelRoomView : MonoBehaviour
    {
        private const string Tag = "DuelRoomView";

        [SerializeField] private GameObject loadingState;

        [SerializeField] private GameObject enterRoomNameState;
        [SerializeField] private TMP_InputField roomNameInputField;
        [SerializeField] private Button enterRoomButton;

        [SerializeField] private GameObject selectDuelistsState;
        [SerializeField] private Dropdown duelistsDropdown;
        [SerializeField] private Button spectateButton;
        [SerializeField] private Button goBackButton;

        [SerializeField] private GameObject errorState;
        [SerializeField] private TMP_Text errorDescriptionText;
        [SerializeField] private Button tryAgainButton;

        [SerializeField] private GameObject waitingState;
        [SerializeField] private Button leaveRoomButton;

        private DuelRoomViewModel _duelRoomViewModel;
        private ISmartDuelServer _smartDuelServer;
        private IAppLogger _logger;

        private IDisposable _smartDuelEventSubscription;

        private CompositeDisposable _disposables = new CompositeDisposable();

        #region Constructors

        [Inject]
        public void Construct(
            DuelRoomViewModel duelRoomViewModel,
            ISmartDuelServer smartDuelServer,
            IAppLogger logger)
        {
            _duelRoomViewModel = duelRoomViewModel;
            _smartDuelServer = smartDuelServer;
            _logger = logger;            

            BindViews();
            InitSmartDuelEventSubscription();
        }

        #endregion

        #region Lifecycle

        private void Awake()
        {
            UpdateDuelRoomState(DuelRoomState.Loading);
        }

        private void OnDestroy()
        {
            _smartDuelEventSubscription?.Dispose();
            _disposables?.Dispose();
            _smartDuelEventSubscription = null;

            _duelRoomViewModel?.Dispose();
        }

        #endregion

        #region Initialization

        private void UpdateDuelRoomState(DuelRoomState state)
        {
            _logger.Log(Tag, $"UpdateDuelRoomState(state: {state})");

            loadingState.SetActive(state == DuelRoomState.Loading);
            enterRoomNameState.SetActive(state == DuelRoomState.EnterRoomName);
            selectDuelistsState.SetActive(state == DuelRoomState.SelectDuelist);
            errorState.SetActive(state == DuelRoomState.Error);
            waitingState.SetActive(state == DuelRoomState.Waiting);
        }

        private void BindViews()
        {
            // Buttons
            enterRoomButton.OnClickAsObservable()
                .Subscribe(_ => _duelRoomViewModel.OnEnterRoomPressed());
            spectateButton.OnClickAsObservable()
                .Subscribe(_ => _duelRoomViewModel.OnSpectateButtonPressed(duelistsDropdown.options[duelistsDropdown.value].text));
            goBackButton.OnClickAsObservable()
                .Subscribe(_ => _duelRoomViewModel.OnGoBackButtonPressed());
            tryAgainButton.OnClickAsObservable()
                .Subscribe(async _ => await _duelRoomViewModel.OnTryAgainButtonPressed());
            leaveRoomButton.OnClickAsObservable()
                .Subscribe(_ => _duelRoomViewModel.OnLeaveRoomButtonPressed());

            // Input Fields
            _disposables.Add(roomNameInputField.onValueChanged.AsObservable()
                .Subscribe(text => _duelRoomViewModel.UpdateRoomName(text)));
            _disposables.Add(duelistsDropdown.options.ToObservable()
                .Subscribe(value => _duelRoomViewModel.UpdateDuelistToSpectate(value.text)));

            // VM Streams
            _disposables.Add(_duelRoomViewModel.UpdateRoomNameField
                .Subscribe(text => UpdateRoomNameField(text)));
            _disposables.Add(_duelRoomViewModel.UpdateDuelRoomState
                .Subscribe(state => UpdateDuelRoomState(state)));
            _disposables.Add(_duelRoomViewModel.UpdateErrorTextField
                .Subscribe(e => UpdateErrorText(e)));
            _disposables.Add(_duelRoomViewModel.UpdateDropDownMenu
                .Subscribe(data => UpdateDropdownMenu(data)));
        }

        private void InitSmartDuelEventSubscription()
        {
            _smartDuelEventSubscription = _smartDuelServer.GlobalEvents
                .Merge(_smartDuelServer.RoomEvents)
                .Subscribe(e => _duelRoomViewModel.OnSmartDuelEventReceived(e));

            _smartDuelServer.Init();
        }

        #endregion

        #region Form Fields

        private void UpdateRoomNameField(string roomName)
        {
            roomNameInputField.text = roomName;
        }

        private void UpdateErrorText(string error)
        {
            errorDescriptionText.text = $"Could not connect to Smart Duel Server\nReason: {error}";
        }

        private void UpdateDropdownMenu(RoomEventData data)
        {
            duelistsDropdown.ResetDropdown(data);
        }

        #endregion
    }
}