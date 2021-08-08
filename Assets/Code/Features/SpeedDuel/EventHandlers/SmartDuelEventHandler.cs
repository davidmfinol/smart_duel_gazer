using System;
using System.Linq;
using Code.Core.DataManager;
using Code.Core.DataManager.GameObjects.Entities;
using Code.Core.Dialog;
using Code.Core.Dialog.Entities;
using Code.Core.Logger;
using Code.Core.Navigation;
using Code.Core.Screen;
using Code.Core.SmartDuelServer;
using Code.Core.SmartDuelServer.Entities;
using Code.Core.SmartDuelServer.Entities.EventData.CardEvents;
using Code.Core.SmartDuelServer.Entities.EventData.RoomEvents;
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
        private const string Tag = "SmartDuelEventHandler";
        private const int TokenId = 73915052;

        private ISmartDuelServer _smartDuelServer;
        private IDataManager _dataManager;
        private IDialogService _dialogService;
        private INavigationService _navigationService;
        private ICreatePlayerStateUseCase _createPlayerStateUseCase;
        private ICreatePlayCardUseCase _createPlayCardUseCase;
        private IMoveCardInteractor _moveCardInteractor;
        private IAppLogger _logger;

        private Core.SmartDuelServer.Entities.EventData.RoomEvents.DuelRoom _duelRoom;
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
            INavigationService navigationService,
            ICreatePlayerStateUseCase createPlayerStateUseCase,
            ICreatePlayCardUseCase createPlayCardUseCase,
            IMoveCardInteractor moveCardInteractor,
            IAppLogger logger)
        {
            _smartDuelServer = smartDuelServer;
            _dataManager = dataManager;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _createPlayerStateUseCase = createPlayerStateUseCase;
            _createPlayCardUseCase = createPlayCardUseCase;
            _moveCardInteractor = moveCardInteractor;
            _logger = logger;

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
            _logger.Log(Tag, $"OnSmartDuelEventReceived(scope: {e.Scope}, action: {e.Action})");

            FetchSpeedDuelFieldIfNecessary();
            if (_speedDuelField == null)
            {
                _logger.Warning(Tag, "Speed Duel Field isn't placed yet");
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
            if (_speedDuelField != null) return;

            _speedDuelField = FindObjectOfType<PlacementEventHandler>().SpeedDuelField;
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

                case SmartDuelEventConstants.CardRemoveAction:
                    HandleRemoveCardEvent(data);
                    return;
            }
        }

        private void HandlePlayCardEvent(CardEventData data)
        {
            _logger.Log(Tag, $"HandlePlayCardEvent(duelistId: {data.DuelistId}, cardId: {data.CardId})");

            if (data.CardPosition == null)
            {
                return;
            }

            var playerState = _speedDuelState.GetPlayerStates().First(ps => ps.DuelistId == data.DuelistId);
            var playCard = playerState.GetCards()
                .FirstOrDefault(card => card.Id == data.CardId && card.CopyNumber == data.CopyNumber);

            if (playCard == null)
            {
                var tokenCount = playerState.GetCards().Count(card => card.Id == TokenId);
                playCard = _createPlayCardUseCase.Execute(TokenId, tokenCount + 1);
            }

            var newZone = playerState.GetZones().FirstOrDefault(zone => zone.ZoneType == data.ZoneType);
            var updatedPlayerState =
                _moveCardInteractor.Execute(playerState, playCard, data.CardPosition.Value, newZone, _speedDuelField);
            UpdateSpeedDuelState(playerState, updatedPlayerState);
        }

        private void HandleRemoveCardEvent(CardEventData data)
        {
            _logger.Log(Tag, $"HandleRemoveCardEvent(duelistId: {data.DuelistId}, cardId: {data.CardId})");

            var playerState = _speedDuelState.GetPlayerStates().First(ps => ps.DuelistId == data.DuelistId);
            var playCard = playerState.GetCards()
                .FirstOrDefault(card => card.Id == data.CardId && card.CopyNumber == data.CopyNumber);

            var updatedPlayerState =
                _moveCardInteractor.Execute(playerState, playCard, CardPosition.Destroy);
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

            _dialogService.ShowDialog(new DialogConfig
            {
                Title = "Duel is over",
                Description = $"{winnerId} won the duel!",
                PositiveText = "Continue",
                PositiveAction = () => ExecuteEndOfGame()
            });
        }

        #endregion

        //Handle Async functions that haven't completed yet
        private void ExecuteEndOfGame()
        {
            _dataManager.RemoveGameObject(GameObjectKeys.ParticlesKey);
            _dataManager.RemoveGameObject(GameObjectKeys.SetCardKey);
            _dataManager.RemoveGameObject(GameObjectKeys.PlayfieldKey);

            Destroy(_speedDuelField);

            _navigationService.ShowConnectionScene();
        }

        #endregion
    }
}