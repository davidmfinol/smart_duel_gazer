using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.Dialog.Interface;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelComponentsManager;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler;
using AssemblyCSharp.Assets.Code.Core.Models.Interface.ModelEventsHandler.Entities;
using AssemblyCSharp.Assets.Code.Core.Screen.Interface;
using AssemblyCSharp.Assets.Code.Features.SpeedDuel.EventHandlers;
using Code.Core.SmartDuelServer.Interface;
using Code.Core.SmartDuelServer.Interface.Entities;
using Code.Core.SmartDuelServer.Interface.Entities.EventData.CardEvents;
using Code.Core.SmartDuelServer.Interface.Entities.EventData.RoomEvents;
using Code.Features.SpeedDuel.Models;
using Code.Features.SpeedDuel.Models.Zones;
using UniRx;
using UnityEngine;
using Zenject;

namespace Code.Features.SpeedDuel.EventHandlers
{
    public class SmartDuelEventHandler : MonoBehaviour
    {
        private const float RemoveCardDurationInSeconds = 7;

        private const string UserPlayMatZonesPath = "UserPlayMat/Zones";
        private const string OpponentPlayMatZonesPath = "OpponentPlayMat/Zones";
        private const string SetCardKey = "SetCard";
        private const string ParticlesKey = "Particles";

        private ISmartDuelServer _smartDuelServer;
        private IDataManager _dataManager;
        private IDialogService _dialogService;
        private ModelEventHandler _modelEventHandler;
        private ModelComponentsManager.Factory _modelFactory;

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
            ModelComponentsManager.Factory modelFactory)
        {
            _smartDuelServer = smartDuelServer;
            _dataManager = dataManager;
            _dialogService = dialogService;
            _modelEventHandler = modelEventHandler;
            _modelFactory = modelFactory;

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
            var userState = CreatePlayerState(user, false);

            var opponent = _duelRoom.Duelists.First(duelist => !duelist.Id.Equals(_duelRoom.DuelistToSpectate));
            var opponentState = CreatePlayerState(opponent, true);

            _speedDuelState = new SpeedDuelState(userState, opponentState);
        }

        private static PlayerState CreatePlayerState(Duelist duelist, bool isOpponent)
        {
            var playCards = new List<PlayCard>();
            foreach (var cardId in duelist.DeckList)
            {
                var copyNumber = playCards.Count(playCard => playCard.Id == cardId);
                playCards.Add(new PlayCard(cardId, copyNumber));
            }

            var playMatZonesPath = isOpponent ? OpponentPlayMatZonesPath : UserPlayMatZonesPath;

            // TODO: when card type is available, move the correct cards to the extra deck
            var playerState = new PlayerState(duelist.Id, isOpponent, playMatZonesPath);
            playerState = playerState.CopyWith(
                deckZone: playerState.DeckZone.CopyWith(playCards)
            );

            return playerState;
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

            var playerState = _speedDuelState.GetPlayerStates().FirstOrDefault(ps => ps.DuelistId == data.DuelistId);
            var zone = playerState?.GetZone(data.ZoneType);
            // TODO: handle cards in multi card zones
            if (zone == null || zone is MultiCardZone)
            {
                return;
            }

            var playMatZonePath = $"{playerState.PlayMatZonesPath}/{data.ZoneType.ToString()}";
            var playMatZone = _speedDuelField.transform.Find(playMatZonePath);
            if (playMatZone == null)
            {
                return;
            }

            PlayCardImage(data, playerState, zone, playMatZone);

            /*var cardModel = _dataManager.GetCardModel(data.CardId);
            if (cardModel == null)
            {
                // TODO: create a function which returns either an instantiated 3D model of the monster
                // or an instantiated model of the card, and then handle the position
                PlayCardImage(data, zone);
                return;
            }

            cardModel.SetActive(true);
            PlayCardModel(data, zone, cardModel);*/
        }

        #region Card image

        private void PlayCardImage(CardEventData data, PlayerState playerState, Zone zone, Transform playMatZone)
        {
            // TODO: handle cards in multi card zones
            if (!(zone is SingleCardZone cardZone))
            {
                return;
            }

            if (cardZone.SetCardModel == null)
            {
                var playCard = new PlayCard(data.CardId, data.CopyNumber);
                var setCardModel = GetGameObject(SetCardKey, playMatZone.position, playMatZone.rotation);

                var updatedZone = cardZone.CopyWith(playCard, setCardModel);
                var currentZones = playerState.GetZones().ToList();
                currentZones.RemoveAll(z => z.ZoneType == data.ZoneType);
                currentZones.Add(updatedZone);

                var updatedPlayerState = playerState.CopyWith(
                    currentZones.First(z => z.ZoneType == ZoneType.Hand) as MultiCardZone,
                    currentZones.First(z => z.ZoneType == ZoneType.Field) as SingleCardZone,
                    currentZones.First(z => z.ZoneType == ZoneType.MainMonster1) as SingleCardZone,
                    currentZones.First(z => z.ZoneType == ZoneType.MainMonster2) as SingleCardZone,
                    currentZones.First(z => z.ZoneType == ZoneType.MainMonster3) as SingleCardZone,
                    currentZones.First(z => z.ZoneType == ZoneType.Graveyard) as MultiCardZone,
                    currentZones.First(z => z.ZoneType == ZoneType.Banished) as MultiCardZone,
                    currentZones.First(z => z.ZoneType == ZoneType.ExtraDeck) as MultiCardZone,
                    currentZones.First(z => z.ZoneType == ZoneType.SpellTrap1) as SingleCardZone,
                    currentZones.First(z => z.ZoneType == ZoneType.SpellTrap2) as SingleCardZone,
                    currentZones.First(z => z.ZoneType == ZoneType.SpellTrap3) as SingleCardZone,
                    currentZones.First(z => z.ZoneType == ZoneType.Deck) as MultiCardZone
                );

                var currentSpeedDuelState = _speedDuelState;
                var playerStates = currentSpeedDuelState.GetPlayerStates().ToList();
                playerStates.Remove(playerState);
                playerStates.Add(updatedPlayerState);

                var updatedSpeedDuelState = _speedDuelState.CopyWith(
                    playerStates.First(ps => !ps.IsOpponent),
                    playerStates.First(ps => ps.IsOpponent)
                );

                _speedDuelState = updatedSpeedDuelState;

                // TODO: Ask Subtle why this is needed
                if (data.CardPosition == CardPosition.FaceDown)
                {
                    HandlePlayCardModelEvents(default, playMatZone.name, data.CardId, false);
                    return;
                }
            }

            switch (data.CardPosition)
            {
                case CardPosition.FaceUp:
                    HandlePlayCardModelEvents(ModelEvent.SpellTrapActivate, playMatZone.name, data.CardId, false);
                    break;
                case CardPosition.FaceDown:
                    HandlePlayCardModelEvents(ModelEvent.ReturnToFaceDown, playMatZone.name, data.CardId, false);
                    break;
                case CardPosition.FaceUpDefence:
                    HandlePlayCardModelEvents(ModelEvent.RevealSetMonster, playMatZone.name, data.CardId, true);
                    break;
                case CardPosition.FaceDownDefence:
                    HandlePlayCardModelEvents(default, playMatZone.name, data.CardId, true);
                    break;
            }
        }

        #endregion

        #region Card model

        /*private void PlayCardModel(CardEventData data, Transform zone, GameObject cardModel)
        {
            var instantiatedModel = GetInstantiatedModel(cardModel, zone);
            var hasSetCard = InstantiatedModels.TryGetValue($"{zone.name}:{SetCardKey}", out var setCardModel);

            switch (data.CardPosition)
            {
                case CardPosition.FaceUp:
                    HandleFaceUpPosition(instantiatedModel, zone, hasSetCard, setCardModel);
                    break;
                case CardPosition.FaceUpDefence:
                    HandleFaceUpDefencePosition(instantiatedModel, zone, hasSetCard, setCardModel);
                    break;
                case CardPosition.FaceDownDefence:
                    HandleFaceDownDefencePosition(zone, hasSetCard, instantiatedModel.name);
                    break;
            }
        }

        private GameObject GetInstantiatedModel(GameObject cardModel, Transform zone)
        {
            var alreadyInstantiated = InstantiatedModels.TryGetValue(zone.name, out var instantiatedModel);
            if (alreadyInstantiated)
            {
                return instantiatedModel;
            }

            return InstantiateModel(cardModel, zone);
        }

        private GameObject InstantiateModel(GameObject cardModel, Transform zone)
        {
            Debug.Log($"InstantiateModel(cardModel: {cardModel}, zone: {zone})");

            var instantiatedModel = cardModel.IsClone()
                ? cardModel
                : _modelFactory.Create(cardModel).gameObject.transform.parent.gameObject;

            instantiatedModel.transform.SetParent(_speedDuelField.transform);
            instantiatedModel.transform.SetPositionAndRotation(zone.position, zone.rotation);

            _modelEventHandler.ActivateModel(zone.name);
            InstantiatedModels.Add(zone.name, instantiatedModel);

            return instantiatedModel;
        }

        private void HandleFaceUpPosition(GameObject instantiatedModel, Transform zone, bool hasSetCard, GameObject setCardModel)
        {
            _modelEventHandler.RaiseEventByEventName(ModelEvent.SummonMonster, zone.name);
            instantiatedModel.transform.position = zone.position;

            if (hasSetCard)
            {
                _modelEventHandler.RaiseEventByEventName(ModelEvent.SetCardRemove, zone.name);
                InstantiatedModels.Remove($"{zone.name}:{SetCardKey}");
                setCardModel.SetActive(false);
                _dataManager.SaveGameObject(SetCardKey, setCardModel);
            }
        }

        private void HandleFaceDownDefencePosition(Transform zone, bool hasSetCard, string cardId)
        {
            if (!hasSetCard)
            {
                var setCard = GetGameObject(SetCardKey, zone.position, zone.rotation);
                if (setCard == null)
                {
                    Debug.LogWarning("The setCard queue is empty :(");
                    return;
                }

                InstantiatedModels.Add($"{zone.name}:{SetCardKey}", setCard);

                HandlePlayCardModelEvents(default, zone.name, cardId, true);
            }

            _modelEventHandler.RaiseChangeVisibilityEvent(zone.name, false);
        }

        private void HandleFaceUpDefencePosition(GameObject model, Transform zone, bool hasSetCard, GameObject setCardModel)
        {
            if (hasSetCard)
            {
                HandlePlayCardModelEvents(ModelEvent.RevealSetMonster, zone.name, model.name, true);
                _modelEventHandler.RaiseChangeVisibilityEvent(zone.name, true);

                // This puts the model on top of the set card rather than clipping through it.
                model.transform.position = setCardModel.transform.GetChild(0).GetChild(0).position;
                return;
            }

            var setCard = GetGameObject(SetCardKey, zone.position, zone.rotation);
            HandlePlayCardModelEvents(ModelEvent.RevealSetMonster, zone.name, model.name, true);

            // This puts the model on top of the set card rather than clipping through it.
            model.transform.position = setCard.transform.GetChild(0).GetChild(0).position;

            InstantiatedModels.Add($"{zone.name}:{SetCardKey}", setCard);
            _modelEventHandler.RaiseChangeVisibilityEvent(zone.name, true);
        }*/

        #endregion

        private void HandlePlayCardModelEvents(ModelEvent modelEvent, string zone, int cardId, bool isMonster)
        {
            _modelEventHandler.RaiseSummonSetCardEvent(zone, cardId.ToString(), isMonster);

            switch (modelEvent)
            {
                case ModelEvent.SpellTrapActivate:
                    _modelEventHandler.RaiseEventByEventName(ModelEvent.SpellTrapActivate, zone);
                    break;
                case ModelEvent.RevealSetMonster:
                    _modelEventHandler.RaiseEventByEventName(ModelEvent.RevealSetMonster, zone);
                    break;
                case ModelEvent.ReturnToFaceDown:
                    _modelEventHandler.RaiseEventByEventName(ModelEvent.ReturnToFaceDown, zone);
                    break;
            }
        }

        #region Remove card event

        /*private void HandleRemoveCardEvent(CardEventData data)
        {
            Debug.Log($"HandleRemoveCardEvent(duelistId: {data.DuelistId}, cardId: {data.CardId})");

            var modelExists = InstantiatedModels.TryGetValue(data.ZoneType, out var model);
            if (!modelExists)
            {
                RemoveSetCard(data);
                return;
            }

            var destructionParticles = GetGameObject(ParticlesKey, model.transform.position, model.transform.rotation);

            _modelEventHandler.RaiseEventByEventName(ModelEvent.DestroyMonster, data.ZoneType);

            StartCoroutine(RecycleGameObject(ParticlesKey, destructionParticles));
            StartCoroutine(RecycleGameObject(model.name, model));

            InstantiatedModels.Remove(data.ZoneType);

            if (InstantiatedModels.ContainsKey($"{data.ZoneType}:{SetCardKey}"))
            {
                _modelEventHandler.RaiseEventByEventName(ModelEvent.DestroySetMonster, data.ZoneType);
                RemoveSetCard(data);
            }
        }

        private void RemoveSetCard(CardEventData data)
        {
            var isCardSet = InstantiatedModels.TryGetValue($"{data.ZoneType}:{SetCardKey}", out var setCard);
            if (!isCardSet)
            {
                return;
            }

            _modelEventHandler.RaiseEventByEventName(ModelEvent.SetCardRemove, data.ZoneType);
            InstantiatedModels.Remove($"{data.ZoneType}:{SetCardKey}");
            StartCoroutine(RecycleGameObject(SetCardKey, setCard));
        }*/

        #endregion

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

        private GameObject GetGameObject(string key, Vector3 position, Quaternion rotation)
        {
            var model = _dataManager.GetGameObject(key);

            model.transform.SetPositionAndRotation(position, rotation);
            model.SetActive(true);

            return model;
        }

        private IEnumerator RecycleGameObject(string key, GameObject model)
        {
            yield return new WaitForSeconds(RemoveCardDurationInSeconds);

            model.SetActive(false);

            _dataManager.SaveGameObject(key.RemoveCloneSuffix(), model);
        }

        #endregion
    }
}