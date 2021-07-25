using System;
using System.Linq;
using System.Threading.Tasks;
using AssemblyCSharp.Assets.Code.Core.Config.Interface.Providers;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.Dialog.Interface;
using AssemblyCSharp.Assets.Code.Core.Navigation.Interface;
using AssemblyCSharp.Assets.Code.Core.Screen.Interface;
using Code.Core.SmartDuelServer.Interface;
using Code.Core.SmartDuelServer.Interface.Entities;
using Code.Core.SmartDuelServer.Interface.Entities.EventData;
using Code.Core.SmartDuelServer.Interface.Entities.EventData.RoomEvents;
using Code.Features.DuelRoom.Models;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
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

        private IDataManager _dataManager;
        private INavigationService _navigationService;
        private IDialogService _dialogService;
        private ISmartDuelServer _smartDuelServer;
        private IDelayProvider _delayProvider;

        private IDisposable _smartDuelEventSubscription;
        private DuelRoomState _currentState;
        private string _roomName;
        private string _duelistToSpectate;
        private bool _startedDuelSuccessfully;

        #region Constructors

        [Inject]
        public void Construct(
            IDataManager dataManager,
            INavigationService navigationService,
            IDialogService dialogService,
            IScreenService screenService,
            ISmartDuelServer smartDuelServer,
            IDelayProvider delayProvider)
        {
            _dataManager = dataManager;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _smartDuelServer = smartDuelServer;
            _delayProvider = delayProvider;

            screenService.UsePortraitOrientation();
            InitSmartDuelEventSubscription();
        }

        #endregion

        #region Lifecycle

        private void Awake()
        {
            UpdateDuelRoomState(DuelRoomState.Loading);
            InitButtons();
        }

        private void OnDestroy()
        {
            _smartDuelEventSubscription?.Dispose();
            _smartDuelEventSubscription = null;

            if (!_startedDuelSuccessfully)
            {
                _smartDuelServer?.Dispose();
            }
        }

        #endregion

        #region Initialization

        private void UpdateDuelRoomState(DuelRoomState state)
        {
            Debug.Log($"UpdateDuelRoomState(state: {state})");

            _currentState = state;

            loadingState.SetActive(state == DuelRoomState.Loading);
            enterRoomNameState.SetActive(state == DuelRoomState.EnterRoomName);
            selectDuelistsState.SetActive(state == DuelRoomState.SelectDuelist);
            errorState.SetActive(state == DuelRoomState.Error);
            waitingState.SetActive(state == DuelRoomState.Waiting);
        }

        private void InitButtons()
        {
            enterRoomButton.OnClickAsObservable().Subscribe(_ => OnEnterRoomPressed());
            spectateButton.OnClickAsObservable().Subscribe(_ => OnSpectateButtonPressed());
            goBackButton.OnClickAsObservable().Subscribe(_ => OnGoBackButtonPressed());
            tryAgainButton.OnClickAsObservable().Subscribe(async _ => await OnTryAgainButtonPressed());
            leaveRoomButton.OnClickAsObservable().Subscribe(_ => OnLeaveRoomButtonPressed());
        }

        private void InitSmartDuelEventSubscription()
        {
            _smartDuelEventSubscription = _smartDuelServer.GlobalEvents
                .Merge(_smartDuelServer.RoomEvents)
                .Subscribe(OnSmartDuelEventReceived);

            _smartDuelServer.Init();
        }

        #endregion

        #region Button events

        private void OnEnterRoomPressed()
        {
            var roomName = roomNameInputField.text;
            if (string.IsNullOrWhiteSpace(roomName))
            {
                _dialogService.ShowToast("Room name is required");
                return;
            }

            _roomName = roomName;

            SendGetDuelistsInRoomEvent();
        }

        private void OnSpectateButtonPressed()
        {
            var index = duelistsDropdown.value;
            var duelist = duelistsDropdown.options[index].text;
            if (string.IsNullOrWhiteSpace(duelist))
            {
                _dialogService.ShowToast("Duelist name is required");
                return;
            }

            _duelistToSpectate = duelist;

            SendSpectateRoomEvent();
        }

        private void OnGoBackButtonPressed()
        {
            ResetFields();

            UpdateDuelRoomState(DuelRoomState.EnterRoomName);
        }

        private async Task OnTryAgainButtonPressed()
        {
            ResetFields();

            UpdateDuelRoomState(DuelRoomState.Loading);

            _smartDuelServer.Dispose();
            await _delayProvider.Wait(200);
            _smartDuelServer.Init();
        }

        private void OnLeaveRoomButtonPressed()
        {
            ResetFields();

            UpdateDuelRoomState(DuelRoomState.EnterRoomName);
            SendLeaveRoomEvent();
        }

        private void ResetFields()
        {
            _roomName = null;
            _duelistToSpectate = null;

            duelistsDropdown.ClearOptions();
        }

        #endregion

        #region Send smart duel events

        private void SendGetDuelistsInRoomEvent()
        {
            _smartDuelServer.EmitEvent(new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomGetDuelistsAction,
                new RoomEventData(_roomName)));
        }

        private void SendSpectateRoomEvent()
        {
            _smartDuelServer.EmitEvent(new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomSpectateAction,
                new RoomEventData(_roomName)));
        }

        private void SendLeaveRoomEvent()
        {
            _smartDuelServer.EmitEvent(new SmartDuelEvent(
                SmartDuelEventConstants.RoomScope,
                SmartDuelEventConstants.RoomLeaveAction));
        }

        #endregion

        #region Receive smart duel events

        private void OnSmartDuelEventReceived(SmartDuelEvent e)
        {
            Debug.Log($"OnSmartDuelEventReceived(scope: {e.Scope}, action: {e.Action})");

            switch (e.Scope)
            {
                case SmartDuelEventConstants.GlobalScope:
                    HandleGlobalEvent(e);
                    break;

                case SmartDuelEventConstants.RoomScope:
                    HandleRoomEvent(e);
                    break;
            }
        }

        #region Receive global event

        private void HandleGlobalEvent(SmartDuelEvent e)
        {
            switch (e.Action)
            {
                case SmartDuelEventConstants.GlobalConnectAction:
                    HandleConnectEvent();
                    break;

                case SmartDuelEventConstants.GlobalConnectErrorAction:
                case SmartDuelEventConstants.GlobalConnectTimeoutAction:
                case SmartDuelEventConstants.GlobalErrorAction:
                    HandleErrorEvent(e.Action);
                    break;
            }
        }

        private void HandleConnectEvent()
        {
            if (_currentState == DuelRoomState.Loading)
            {
                UpdateDuelRoomState(DuelRoomState.EnterRoomName);
            }
        }

        private void HandleErrorEvent(string error)
        {
            errorDescriptionText.text = $"Could not connect to Smart Duel Server\nReason: {error}";
            UpdateDuelRoomState(DuelRoomState.Error);
        }

        #endregion

        #region Receive room event

        private void HandleRoomEvent(SmartDuelEvent e)
        {
            if (!(e.Data is RoomEventData data))
            {
                return;
            }

            switch (e.Action)
            {
                case SmartDuelEventConstants.RoomGetDuelistsAction:
                    HandleRoomGetDuelistsEvent(data);
                    break;

                case SmartDuelEventConstants.RoomSpectateAction:
                    HandleRoomSpectateEvent();
                    break;

                case SmartDuelEventConstants.RoomStartAction:
                    HandleRoomStartEvent(data);
                    break;

                case SmartDuelEventConstants.RoomCloseAction:
                    HandleRoomCloseEvent();
                    break;
            }
        }

        private void HandleRoomGetDuelistsEvent(RoomEventData data)
        {
            if (!data.Error.IsNullOrEmpty())
            {
                HandleErrorEvent(data.Error);
                return;
            }

            duelistsDropdown.ClearOptions();
            duelistsDropdown.AddOptions(data.DuelistsIds.ToList());

            UpdateDuelRoomState(DuelRoomState.SelectDuelist);
        }

        private void HandleRoomSpectateEvent()
        {
            UpdateDuelRoomState(DuelRoomState.Waiting);
        }

        private void HandleRoomStartEvent(RoomEventData data)
        {
            var duelRoom = data.DuelRoom;
            if (duelRoom == null)
            {
                HandleErrorEvent("invalid duel room data");
                return;
            }

            duelRoom.DuelistToSpectate = _duelistToSpectate;
            _dataManager.SaveDuelRoom(duelRoom);

            _startedDuelSuccessfully = true;
            _navigationService.ShowSpeedDuelScene();
        }

        private void HandleRoomCloseEvent()
        {
            HandleErrorEvent("roomIsClosed");
        }

        #endregion

        #endregion
    }
}