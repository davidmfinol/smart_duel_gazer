using System;
using System.Collections.Generic;
using System.Linq;
using Code.Core.DataManager;
using Code.Core.Dialog;
using Code.Core.Dialog.Entities;
using Code.Core.Localization;
using Code.Core.Localization.Entities;
using Code.Core.Logger;
using Code.Core.Screen;
using Code.Core.SmartDuelServer;
using Code.Core.SmartDuelServer.Entities;
using Code.Core.SmartDuelServer.Entities.EventData.CardEvents;
using Code.Core.SmartDuelServer.Entities.EventData.RoomEvents;
using Code.Features.SpeedDuel.Models;
using Code.Features.SpeedDuel.Models.Zones;
using Code.Features.SpeedDuel.UseCases;
using Code.Features.SpeedDuel.UseCases.CardBattle;
using Code.Features.SpeedDuel.UseCases.MoveCard;
using Code.Features.SpeedDuel.UseCases.MoveCard.ModelsAndEvents;
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
        private ICreatePlayerStateUseCase _createPlayerStateUseCase;
        private ICreatePlayCardUseCase _createPlayCardUseCase;
        private IMoveCardInteractor _moveCardInteractor;
        private IMonsterBattleInteractor _monsterBattleInteractor;
        private IPlayCardInteractor _playCardInteractor;
        private IDeclareCardUseCase _declareCardUseCase;
        private IEndOfDuelUseCase _endOfDuel;
        private IStringProvider _stringProvider;
        private IAppLogger _logger;

        private Core.SmartDuelServer.Entities.EventData.RoomEvents.DuelRoom _duelRoom;
        private SpeedDuelState _speedDuelState;
        private GameObject _speedDuelField;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly IList<SmartDuelEvent> _missedSmartDuelEvents = new List<SmartDuelEvent>();

        #region Constructors

        [Inject]
        public void Construct(
            ISmartDuelServer smartDuelServer,
            IDataManager dataManager,
            IScreenService screenService,
            IDialogService dialogService,
            ICreatePlayerStateUseCase createPlayerStateUseCase,
            ICreatePlayCardUseCase createPlayCardUseCase,
            IMoveCardInteractor moveCardInteractor,
            IMonsterBattleInteractor monsterBattleInteractor,
            IPlayCardInteractor playCardInteractor,
            IDeclareCardUseCase declareCardUseCase,
            IEndOfDuelUseCase endOfDuel,
            IStringProvider stringProvider,
            IAppLogger logger)
        {
            _smartDuelServer = smartDuelServer;
            _dataManager = dataManager;
            _dialogService = dialogService;
            _createPlayerStateUseCase = createPlayerStateUseCase;
            _createPlayCardUseCase = createPlayCardUseCase;
            _moveCardInteractor = moveCardInteractor;
            _monsterBattleInteractor = monsterBattleInteractor;
            _playCardInteractor = playCardInteractor;
            _declareCardUseCase = declareCardUseCase;
            _endOfDuel = endOfDuel;
            _stringProvider = stringProvider;
            _logger = logger;

            screenService.UseAutoOrientation();

            InitSpeedDuelState();
            InitSubscriptions();
        }

        #endregion

        #region Lifecycle

        private void OnDestroy()
        {
            _disposables.Dispose();

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

        private void InitSubscriptions()
        {
            _disposables.Add(_dataManager.PlayfieldStream.Subscribe(OnPlayfieldUpdated));

            _disposables.Add(_smartDuelServer.CardEvents
                .Merge(_smartDuelServer.RoomEvents)
                .Subscribe(OnSmartDuelEventReceived));
        }

        #endregion

        #region Playfield

        private void OnPlayfieldUpdated(GameObject playfield)
        {
            _logger.Log(Tag, "OnPlayfieldUpdated()");

            if (playfield == null)
            {
                return;
            }
            
            _speedDuelField = playfield;
            InitPlayfieldState(_speedDuelField);
            HandleMissedSmartDuelEvents();
        }

        private void InitPlayfieldState(GameObject playfield)
        {
            _logger.Log(Tag, "InitPlayfieldState()");

            foreach (var oldPlayerState in _speedDuelState.GetPlayerStates())
            {
                var oldZones = oldPlayerState.GetZones();
                var newZones = oldZones.ToList();

                foreach (var oldZone in oldZones)
                {
                    if (!(oldZone is SingleCardZone singleCardZone) || singleCardZone.Card == null)
                    {
                        continue;
                    }

                    var newZone = _playCardInteractor.Execute(oldPlayerState, singleCardZone, singleCardZone.Card, playfield);
                    newZones.Remove(oldZone);
                    newZones.Add(newZone);
                }

                var newPlayerState = oldPlayerState.CopyWith(newZones);
                UpdateSpeedDuelState(oldPlayerState, newPlayerState);
            }
        }

        private void HandleMissedSmartDuelEvents()
        {
            foreach (var smartDuelEvent in _missedSmartDuelEvents)
            {
                OnSmartDuelEventReceived(smartDuelEvent);
            }
            
            _missedSmartDuelEvents.Clear();
        }

        #endregion

        #region Receive smart duel events

        private void OnSmartDuelEventReceived(SmartDuelEvent e)
        {
            _logger.Log(Tag, $"OnSmartDuelEventReceived(scope: {e.Scope}, action: {e.Action})");

            if (_speedDuelField == null)
            {
                _missedSmartDuelEvents.Add(e);
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
                    break;

                case SmartDuelEventConstants.CardAttackAction:
                    HandleAttackCardEvent(data);
                    break;
                case SmartDuelEventConstants.CardDeclareAction:
                    HandleDeclareCardEvent(data);
                    break;
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
                .FirstOrDefault(card => card.YugiohCard.Id == data.CardId && card.CopyNumber == data.CopyNumber);

            if (playCard == null)
            {
                var tokenCount = playerState.GetCards().Count(card => card.YugiohCard.Id == TokenId);
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
                .FirstOrDefault(card => card.YugiohCard.Id == data.CardId && card.CopyNumber == data.CopyNumber);

            var updatedPlayerState =
                _moveCardInteractor.Execute(playerState, playCard, CardPosition.Destroy);
            UpdateSpeedDuelState(playerState, updatedPlayerState);
        }

        private void HandleAttackCardEvent(CardEventData data)
        {
            _logger.Log(Tag,
                $"HandleAttackCardEvent(duelistId: {data.DuelistId}, cardId: {data.CardId}), zoneType: {data.ZoneType})");

            if (!data.ZoneType.HasValue)
            {
                return;
            }

            var playerStates = _speedDuelState.GetPlayerStates();

            var attackerState = playerStates.First(ps => ps.DuelistId == data.DuelistId);
            var attackingCard = attackerState.GetCards()
                .FirstOrDefault(card => card.YugiohCard.Id == data.CardId && card.CopyNumber == data.CopyNumber);
            var attackZone = attackingCard == null ? null : attackerState.GetZone(attackingCard.ZoneType);

            var targetPlayerState = playerStates.First(ps => ps.DuelistId != data.DuelistId);
            var targetZone = targetPlayerState.GetZone(data.ZoneType.Value);

            _monsterBattleInteractor.Execute(attackZone, targetZone, targetPlayerState);
        }

        private void HandleDeclareCardEvent(CardEventData data)
        {
            _logger.Log(Tag, 
                $"HandleDeclareCardEvent(duelistId: {data.DuelistId}, cardId: {data.CardId}), copyNumber: {data.CopyNumber}))");

            var playerStates = _speedDuelState.GetPlayerStates();

            var declaredPlayerState = playerStates.First(ps => ps.DuelistId == data.DuelistId);
            var declaredCard = declaredPlayerState.GetCards()
                .FirstOrDefault(card => card.YugiohCard.Id == data.CardId && card.CopyNumber == data.CopyNumber);
            var declaredCardZone = declaredCard == null ? null : declaredPlayerState.GetZone(declaredCard.ZoneType);

            _declareCardUseCase.Execute(declaredCardZone);
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
                Title = _stringProvider.GetString(LocaleKeys.SpeedDuelDuelOverDialogTitle),
                Description = _stringProvider.GetString(LocaleKeys.SpeedDuelDuelOverDialogDescription, winnerId),
                PositiveText = _stringProvider.GetString(LocaleKeys.GeneralActionContinue),
                PositiveAction = () => _endOfDuel.Execute()
            });
        }

        #endregion

        #endregion
    }
}