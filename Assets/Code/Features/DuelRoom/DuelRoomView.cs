using Code.Core.Logger;
using Code.Features.DuelRoom.Models;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.Features.DuelRoom
{
    public class DuelRoomView : MonoBehaviour
    {
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

        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        #region Lifecycle

        [Inject]
        public void Construct(
            DuelRoomViewModel duelRoomViewModel,
            IAppLogger logger)
        {
            _duelRoomViewModel = duelRoomViewModel;

            OnViewModelSet();
        }

        private void Awake()
        {
            UpdateDuelRoomState(DuelRoomState.Loading);
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
            _duelRoomViewModel?.Dispose();
        }

        #endregion

        #region Initialization

        private void OnViewModelSet()
        {
            _duelRoomViewModel.Init();

            BindViews();
        }

        private void BindViews()
        {
            // Buttons
            enterRoomButton.OnClickAsObservable()
                .Subscribe(_ => _duelRoomViewModel.OnEnterRoomPressed());
            spectateButton.OnClickAsObservable()
                .Subscribe(_ => OnSpectateButtonPressed());
            goBackButton.OnClickAsObservable()
                .Subscribe(_ => _duelRoomViewModel.OnGoBackButtonPressed());
            tryAgainButton.OnClickAsObservable()
                .Subscribe(async _ => await _duelRoomViewModel.OnTryAgainButtonPressed());
            leaveRoomButton.OnClickAsObservable()
                .Subscribe(_ => _duelRoomViewModel.OnLeaveRoomButtonPressed());

            // Input Fields
            _disposables.Add(roomNameInputField.onValueChanged.AsObservable()
                .Subscribe(text => _duelRoomViewModel.UpdateRoomName(text)));

            // VM Streams
            _disposables.Add(_duelRoomViewModel.RoomName
                .Subscribe(UpdateRoomNameField));
            _disposables.Add(_duelRoomViewModel.RoomState
                .Subscribe(UpdateDuelRoomState));
            _disposables.Add(_duelRoomViewModel.ErrorText
                .Subscribe(UpdateErrorText));
            _disposables.Add(_duelRoomViewModel.DuelistIds
                .Subscribe(UpdateDropdownMenu));
        }

        private void OnSpectateButtonPressed()
        {
            var duelistToSpectate = duelistsDropdown.options[duelistsDropdown.value].text;

            _duelRoomViewModel.OnSpectateButtonPressed(duelistToSpectate);
        }

        private void UpdateDuelRoomState(DuelRoomState state)
        {
            loadingState.SetActive(state == DuelRoomState.Loading);
            enterRoomNameState.SetActive(state == DuelRoomState.EnterRoomName);
            selectDuelistsState.SetActive(state == DuelRoomState.SelectDuelist);
            errorState.SetActive(state == DuelRoomState.Error);
            waitingState.SetActive(state == DuelRoomState.Waiting);
        }

        #endregion

        #region Form Fields

        private void UpdateRoomNameField(string roomName)
        {
            roomNameInputField.text = roomName;
        }

        private void UpdateErrorText(string errorText)
        {
            errorDescriptionText.text = errorText;
        }

        private void UpdateDropdownMenu(List<string> duelistIds)
        {
            duelistsDropdown.ClearOptions();
            if (duelistIds == null)
            {
                return;
            }

            duelistsDropdown.AddOptions(duelistIds);
        }

        #endregion
    }
}