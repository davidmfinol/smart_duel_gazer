using Zenject;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp.Assets.Code.Core.Screen.Interface;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler;
using AssemblyCSharp.Assets.Code.Core.Models.Interface.ModelEventsHandler.Entities;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelComponentsManager;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;
using UniRx;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities.EventData;

namespace AssemblyCSharp.Assets.Code.Features.SpeedDuel.EventHandlers
{
    public class SmartDuelEventHandler : MonoBehaviour
    {
        private const float RemoveCardDurationInSeconds = 7;

        private const string PlaymatZonesPath = "Playmat/Zones";
        private const string SetCardKey = "SetCard";
        private const string ParticlesKey = "Particles";

        private ISmartDuelServer _smartDuelServer;
        private IDataManager _dataManager;
        private ModelEventHandler _modelEventHandler;
        private ModelComponentsManager.Factory _modelFactory;

        private IDisposable _smartDuelEventSubscription;

        private GameObject _speedDuelField;

        #region Properties

        // TODO: work more OO: create PlayMat/Field entity with zones which contain models
        private IDictionary<string, GameObject> InstantiatedModels { get; } = new Dictionary<string, GameObject>();

        #endregion

        #region Constructors

        [Inject]
        public void Construct(
            ISmartDuelServer smartDuelServer,
            IDataManager dataManager,
            IScreenService screenService,
            ModelEventHandler modelEventHandler,
            ModelComponentsManager.Factory modelFactory)
        {
            _smartDuelServer = smartDuelServer;
            _dataManager = dataManager;
            _modelEventHandler = modelEventHandler;
            _modelFactory = modelFactory;

            screenService.UseAutoOrientation();

            ConnectToServer();
        }

        private void ConnectToServer()
        {
            _smartDuelServer.Init();
        }

        private void InitSmartDuelEventSubscription()
        {
            _smartDuelEventSubscription = Observable
                .Merge(_smartDuelServer.CardEvents, _smartDuelServer.RoomEvents)
                .Subscribe(OnSmartDuelEventReceived);
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

        #region Receive smart duel events

        public void OnSmartDuelEventReceived(SmartDuelEvent e)
        {
            FetchSpeedDuelField();
            if (_speedDuelField == null)
            {
                Debug.LogWarning("Speed Duel Field isn't placed yet");
                return;
            }

            switch (e.Scope)
            {
                case SmartDuelEventConstants.cardScope:
                    HandleCardEvent(e);
                    break;
                // TODO:
                //case SmartDuelEventConstants.roomScope:
                //    HandleRoomEvent(e);
                //    break;
            }
        }

        private void FetchSpeedDuelField()
        {
            if (_speedDuelField == null)
            {
                _speedDuelField = GetComponent<PlacementEventHandler>().SpeedDuelField;
            }
        }

        #region Handle card events

        private void HandleCardEvent(SmartDuelEvent e)
        {
            var eventData = e.Data;
            if (eventData is CardEventData)
            {
                return;
            }

            switch (e.Action)
            {
                case SmartDuelEventConstants.cardPlayAction:
                    HandlePlayCardEvent(eventData as CardEventData);
                    break;
                case SmartDuelEventConstants.cardRemoveAction:
                    HandleRemoveCardEvent(eventData as CardEventData);
                    return;
            }
        }

        private void HandlePlayCardEvent(CardEventData data)
        {
            var zone = _speedDuelField.transform.Find($"{PlaymatZonesPath}/{data.ZoneName}");
            if (zone == null)
            {
                return;
            }

            var cardModel = _dataManager.GetCardModel(data.CardId);
            if (cardModel == null)
            {
                // TODO: create a function which returns either an instantiated 3D model of the monster
                // or an instantiated model of the card, and then handle the position
                PlayCardImage(data, zone);
                return;
            }

            cardModel.SetActive(true);
            PlayCardModel(data, zone, cardModel);
        }

        #region Card image

        private void PlayCardImage(CardEventData data, Transform zone)
        {
            var setCardKey = $"{zone.name}:{SetCardKey}";
            if (!InstantiatedModels.ContainsKey(setCardKey))
            {
                var setCard = GetGameObject(SetCardKey, zone.position, zone.rotation);
                InstantiatedModels.Add(setCardKey, setCard);

                if (data.CardPosition == CardPosition.FaceDown)
                {
                    HandlePlayCardModelEvents(default, zone.name, data.CardId, false);
                    return;
                }
            }

            switch (data.CardPosition)
            {
                case CardPosition.FaceUp:
                    HandlePlayCardModelEvents(ModelEvent.SpellTrapActivate, zone.name, data.CardId, false);
                    break;
                case CardPosition.FaceDown:
                    HandlePlayCardModelEvents(ModelEvent.ReturnToFaceDown, zone.name, data.CardId, false);
                    break;
                case CardPosition.FaceUpDefence:
                    HandlePlayCardModelEvents(ModelEvent.RevealSetMonster, zone.name, data.CardId, true);
                    break;
                case CardPosition.FaceDownDefence:
                    HandlePlayCardModelEvents(default, zone.name, data.CardId, true);
                    break;
            }
        }

        #endregion

        #region Card model

        private void PlayCardModel(CardEventData data, Transform zone, GameObject cardModel)
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
        }

        #endregion

        private void HandlePlayCardModelEvents(ModelEvent modelEvent, string zone, string cardId, bool isMonster)
        {
            _modelEventHandler.RaiseSummonSetCardEvent(zone, cardId, isMonster);

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

        private void HandleRemoveCardEvent(CardEventData data)
        {
            var modelExists = InstantiatedModels.TryGetValue(data.ZoneName, out var model);
            if (!modelExists)
            {
                RemoveSetCard(data);
                return;
            }

            var destructionParticles = GetGameObject(ParticlesKey, model.transform.position, model.transform.rotation);

            _modelEventHandler.RaiseEventByEventName(ModelEvent.DestroyMonster, data.ZoneName);

            StartCoroutine(RecycleGameObject(ParticlesKey, destructionParticles));
            StartCoroutine(RecycleGameObject(model.name, model));

            InstantiatedModels.Remove(data.ZoneName);

            if (InstantiatedModels.ContainsKey($"{data.ZoneName}:{SetCardKey}"))
            {
                _modelEventHandler.RaiseEventByEventName(ModelEvent.DestroySetMonster, data.ZoneName);
                RemoveSetCard(data);
            }
        }

        private void RemoveSetCard(CardEventData data)
        {
            var isCardSet = InstantiatedModels.TryGetValue($"{data.ZoneName}:{SetCardKey}", out var setCard);
            if (!isCardSet)
            {
                return;
            }

            _modelEventHandler.RaiseEventByEventName(ModelEvent.SetCardRemove, data.ZoneName);
            InstantiatedModels.Remove($"{data.ZoneName}:{SetCardKey}");
            StartCoroutine(RecycleGameObject(SetCardKey, setCard));
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
