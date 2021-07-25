using System;
using System.Collections;
using System.Linq;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.Dialog.Interface;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelComponentsManager;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler;
using AssemblyCSharp.Assets.Code.Core.Screen.Interface;
using AssemblyCSharp.Assets.Code.Features.SpeedDuel.EventHandlers;
using Code.Core.SmartDuelServer.Interface;
using Code.Core.SmartDuelServer.Interface.Entities;
using Code.Core.SmartDuelServer.Interface.Entities.EventData.CardEvents;
using Code.Core.SmartDuelServer.Interface.Entities.EventData.RoomEvents;
using Code.Features.SpeedDuel.Models;
using Code.Features.SpeedDuel.UseCases;
using Code.Features.SpeedDuel.UseCases.MoveCard;
using UniRx;
using UnityEngine;
using Zenject;

namespace Code.Features.SpeedDuel.EventHandlers
{
    public class SmartDuelEventHandler : MonoBehaviour
    {
        private const float RemoveCardDurationInSeconds = 7;

        private const string SetCardKey = "SetCard";
        private const string ParticlesKey = "Particles";

        private ISmartDuelServer _smartDuelServer;
        private IDataManager _dataManager;
        private IDialogService _dialogService;
        private ModelEventHandler _modelEventHandler;
        private ModelComponentsManager.Factory _modelFactory;
        private ICreatePlayerStateUseCase _createPlayerStateUseCase;
        private ICreatePlayCardUseCase _createPlayCardUseCase;
        private IMoveCardInteractor _moveCardInteractor;

        private Core.SmartDuelServer.Interface.Entities.EventData.RoomEvents.DuelRoom _duelRoom;
        private SpeedDuelState _speedDuelState;
        private IDisposable _smartDuelEventSubscription;
        private GameObject _speedDuelField;

        #region Constructors

        [Inject]
        public void Construct(
            ISmartDuelServer smartDuelServer,
            IDataManager dataManager,
            IScreenService screenService,
            IDialogService dialogService,
            ModelEventHandler modelEventHandler,
            ModelComponentsManager.Factory modelFactory,
            ICreatePlayerStateUseCase createPlayerStateUseCase,
            ICreatePlayCardUseCase createPlayCardUseCase,
            IMoveCardInteractor moveCardInteractor)
        {
            _smartDuelServer = smartDuelServer;
            _dataManager = dataManager;
            _dialogService = dialogService;
            _modelEventHandler = modelEventHandler;
            _modelFactory = modelFactory;
            _createPlayerStateUseCase = createPlayerStateUseCase;
            _createPlayCardUseCase = createPlayCardUseCase;
            _moveCardInteractor = moveCardInteractor;

            screenService.UseAutoOrientation();

            InitSpeedDuelState();
            InitSmartDuelEventSubscription();
        }

        #endregion

        #region Lifecycle

        private void OnDestroy()
        {
            _smartDuelEventSubscription?.Dispose();
            _smartDuelEventSubscription = null;

            _smartDuelServer?.Dispose();
        }

        #endregion

        #region Initialization

        private void InitSpeedDuelState()
        {
            var duelRoom = _dataManager.GetDuelRoom();
            _duelRoom = duelRoom ?? throw new Exception("Duel room not initialized");

            var user = _duelRoom.Duelists.First(duelist => duelist.Id.Equals(_duelRoom.DuelistToSpectate));
            var userState = _createPlayerStateUseCase.Execute(user, false);

            var opponent = _duelRoom.Duelists.First(duelist => !duelist.Id.Equals(_duelRoom.DuelistToSpectate));
            var opponentState = _createPlayerStateUseCase.Execute(opponent, true);

            _speedDuelState = new SpeedDuelState(userState, opponentState);
        }

        private void InitSmartDuelEventSubscription()
        {
            _smartDuelEventSubscription = _smartDuelServer.CardEvents
                .Merge(_smartDuelServer.RoomEvents)
                .Subscribe(OnSmartDuelEventReceived);
        }

        #endregion

        #region Receive smart duel events

        private void OnSmartDuelEventReceived(SmartDuelEvent e)
        {
            Debug.Log($"OnSmartDuelEventReceived(scope: {e.Scope}, action: {e.Action})");

            FetchSpeedDuelFieldIfNecessary();
            if (_speedDuelField == null)
            {
                Debug.LogWarning("Speed Duel Field isn't placed yet");
                return;
            }

            switch (e.Scope)
            {
                case SmartDuelEventConstants.CardScope:
                    HandleCardEvent(e);
                    break;

                case SmartDuelEventConstants.RoomScope:
                    HandleRoomEvent(e);
                    break;
            }
        }

        private void FetchSpeedDuelFieldIfNecessary()
        {
            if (_speedDuelField == null)
            {
                _speedDuelField = GetComponent<PlacementEventHandler>().SpeedDuelField;
            }
        }

        #region Handle card events

        private void HandleCardEvent(SmartDuelEvent e)
        {
            if (!(e.Data is CardEventData data))
            {
                return;
            }

            switch (e.Action)
            {
                case SmartDuelEventConstants.CardPlayAction:
                    HandlePlayCardEvent(data);
                    break;

                /*case SmartDuelEventConstants.CardRemoveAction:
                    HandleRemoveCardEvent(data);
                    return;*/
            }
        }

        private void HandlePlayCardEvent(CardEventData data)
        {
            Debug.Log($"HandlePlayCardEvent(duelistId: {data.DuelistId}, cardId: {data.CardId})");

            var playerState = _speedDuelState.GetPlayerStates().First(ps => ps.DuelistId == data.DuelistId);
            var playCard = playerState.GetCards()
                .FirstOrDefault(card => card.Id == data.CardId && card.CopyNumber == data.CopyNumber);

            if (playCard == null)
            {
                // TODO: handle token
            }

            var newZone = playerState.GetZones().FirstOrDefault(zone => zone.ZoneType == data.ZoneType);
            var updatedPlayerState = _moveCardInteractor.Execute(playerState, playCard, data.CardPosition, newZone, _speedDuelField);
            UpdateSpeedDuelState(playerState, updatedPlayerState);
        }

        private void UpdateSpeedDuelState(PlayerState oldPlayerState, PlayerState updatedPlayerState)
        {
            var currentSpeedDuelState = _speedDuelState;

            var playerStates = currentSpeedDuelState.GetPlayerStates().ToList();
            playerStates.Remove(oldPlayerState);
            playerStates.Add(updatedPlayerState);

            _speedDuelState = currentSpeedDuelState.CopyWith(
                playerStates.First(ps => !ps.IsOpponent),
                playerStates.First(ps => ps.IsOpponent)
            );
        }

        #endregion

        #region Handle room events

        private void HandleRoomEvent(SmartDuelEvent e)
        {
            if (!(e.Data is RoomEventData data))
            {
                return;
            }

            switch (e.Action)
            {
                case SmartDuelEventConstants.RoomCloseAction:
                    HandleCloseRoomEvent(data);
                    break;
            }
        }

        private void HandleCloseRoomEvent(RoomEventData data)
        {
            var winnerId = data.WinnerId;
            if (winnerId == null)
            {
                return;
            }

            var winnerMessage = $"{winnerId} won the duel!";
            _dialogService.ShowToast(winnerMessage);
        }

        #endregion

        private IEnumerator RecycleGameObject(string key, GameObject model)
        {
            yield return new WaitForSeconds(RemoveCardDurationInSeconds);

            model.SetActive(false);

            _dataManager.SaveGameObject(key.RemoveCloneSuffix(), model);
        }

        #endregion
    }
}