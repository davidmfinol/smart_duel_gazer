using Zenject;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp.Assets.Code.Core.Screen.Interface;
using AssemblyCSharp.Assets.Code.Core.YGOProDeck.Interface;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler;
using AssemblyCSharp.Assets.Code.Core.Models.Interface.ModelEventsHandler.Entities;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelComponentsManager;

namespace AssemblyCSharp.Assets.Code.Features.SpeedDuel
{
    public class SmartDuelEventHandler : MonoBehaviour, ISmartDuelEventListener
    {
        private const string PlaymatZonesPath = "Playmat/Zones";
        private const string SetCard = "SetCard";

        private const string ParticlesModelRecyclerKey = "Particles";
        private const string SetCardModelRecyclerKey = "SetCards";

        // TODO: create enum
        private const string PositionFaceUp = "faceUp";
        private const string PositionFaceDown = "faceDown";
        private const string PositionFaceUpDefence = "faceUpDefence";
        private const string PositionFaceDownDefence = "faceDownDefence";

        private ISmartDuelServer _smartDuelServer;
        private IDataManager _dataManager;
        private IApiWebRequest _webRequest;
        private ModelEventHandler _modelEventHandler;
        private ModelComponentsManager.Factory _modelFactory;

        private GameObject _speedDuelField;
        private WaitForSeconds _waitTime = new WaitForSeconds(7);

        #region Properties

        private Dictionary<string, GameObject> InstantiatedModels { get; } = new Dictionary<string, GameObject>();

        #endregion

        #region Constructors

        [Inject]
        public void Construct(
            ISmartDuelServer smartDuelServer,
            IDataManager dataManager,
            IApiWebRequest webRequest,
            IScreenService screenService,
            ModelEventHandler modelEventHandler,
            ModelComponentsManager.Factory modelFactory)
        {
            _smartDuelServer = smartDuelServer;
            _dataManager = dataManager;
            _webRequest = webRequest;
            _modelEventHandler = modelEventHandler;
            _modelFactory = modelFactory;

            screenService.UseAutoOrientation();
            ConnectToServer();
        }

        private void ConnectToServer()
        {
            _smartDuelServer.Connect(this);
        }

        #endregion

        #region Lifecycle

        private void OnDestroy()
        {
            _smartDuelServer?.Dispose();
        }

        #endregion

        #region ISmartDuelEventListener

        public void OnSmartDuelEventReceived(SmartDuelEvent smartDuelEvent)
        {
            if (smartDuelEvent is PlayCardEvent playCardEvent)
            {
                OnPlayCardEventReceived(playCardEvent);
            }
            else if (smartDuelEvent is RemoveCardEvent removeCardEvent)
            {
                OnRemovecardEventReceived(removeCardEvent);
            }
        }

        private void OnPlayCardEventReceived(PlayCardEvent playCardEvent)
        {
            FetchSpeedDuelField();

            var zone = _speedDuelField.transform.Find($"{PlaymatZonesPath}/{playCardEvent.ZoneName}");
            if (zone == null)
            {
                return;
            }

            var cardModel = _dataManager.GetCardModel(playCardEvent.CardId);
            if (cardModel == null)
            {
                SummonCardWithoutModel(playCardEvent, zone);
                return;
            }

            HandleCardPosition(playCardEvent, zone, cardModel);
        }

        private void FetchSpeedDuelField()
        {
            if (_speedDuelField == null)
            {
                _speedDuelField = GetComponent<PlacementEvents>().SpeedDuelField;
            }
        }

        private void SummonCardWithoutModel(PlayCardEvent playCardEvent, Transform zone)
        {
            if (!InstantiatedModels.TryGetValue($"{zone.name}:{SetCard}", out var _))
            {
                var setCard = _dataManager.GetFromQueue(SetCardModelRecyclerKey, zone.position, zone.rotation, _speedDuelField.transform);
                InstantiatedModels.Add($"{zone.name}:{SetCard}", setCard);
            }

            // TODO: Test with monster for which there's no 3D model
            if (playCardEvent.CardPosition == PositionFaceUp)
            {
                _webRequest.RequestCardImageFromWeb(EventNames.SpellTrapActivate, zone.name, playCardEvent.CardId, false);
            }
            else if (playCardEvent.CardPosition == PositionFaceDown)
            {
                _webRequest.RequestCardImageFromWeb(default, zone.name, playCardEvent.CardId, false);
            }
            else if (playCardEvent.CardPosition == PositionFaceUpDefence)
            {
                _webRequest.RequestCardImageFromWeb(EventNames.RevealSetMonster, zone.name, playCardEvent.CardId, true);
            }
            else if (playCardEvent.CardPosition == PositionFaceDownDefence)
            {
                _webRequest.RequestCardImageFromWeb(default, zone.name, playCardEvent.CardId, true);
            }
        }

        private GameObject GetInstantiatedModel(GameObject cardModel, Transform zone)
        {
            var alreadyInstantiated = InstantiatedModels.TryGetValue(zone.name, out var model);
            if (alreadyInstantiated)
            {
                return model;
            }

            GameObject instantiatedModel;
            if (_dataManager.DoesModelExist(cardModel.name))
            {
                instantiatedModel = _dataManager.GetFromQueue(
                    cardModel.name, zone.position, zone.rotation, _speedDuelField.transform);
            }
            else
            {
                instantiatedModel = _modelFactory.Create(cardModel).gameObject.transform.parent.gameObject;
                instantiatedModel.transform.SetParent(_speedDuelField.transform);
                instantiatedModel.transform.SetPositionAndRotation(zone.position, zone.rotation);
            }

            _modelEventHandler.ActivateModel(zone.name);
            InstantiatedModels.Add(zone.name, instantiatedModel);

            return instantiatedModel;
        }

        private void HandleCardPosition(PlayCardEvent playCardEvent, Transform zone, GameObject cardModel)
        {
            var model = GetInstantiatedModel(cardModel, zone);
            var hasSetCard = InstantiatedModels.TryGetValue($"{zone.name}:{SetCard}", out var setCardModel);

            if (playCardEvent.CardPosition == PositionFaceUp)
            {
                HandleFaceUpPosition(model, zone, hasSetCard, setCardModel);
                return;
            }

            if (playCardEvent.CardPosition == PositionFaceDownDefence)
            {
                HandleFaceDownDefencePosition(zone, hasSetCard, model.name);
                return;
            }

            if (playCardEvent.CardPosition == PositionFaceUpDefence)
            {
                HandleFaceUpDefencePosition(model, zone, hasSetCard, setCardModel, model.name);
            }
        }

        private void HandleFaceUpPosition(GameObject model, Transform zone, bool hasSetCard, GameObject setCardModel)
        {
            _modelEventHandler.RaiseEventByEventName(EventNames.SummonMonster, zone.name);
            model.transform.position = zone.position;

            if (hasSetCard)
            {
                _modelEventHandler.RaiseEventByEventName(EventNames.SetCardRemove, zone.name);
                InstantiatedModels.Remove($"{zone.name}:{SetCard}");
                _dataManager.AddToQueue(SetCardModelRecyclerKey, setCardModel);
            }
        }

        private void HandleFaceDownDefencePosition(Transform zone, bool hasSetCard, string cardId)
        {
            if (_dataManager.GetCardModel(SetCard) == null)
            {
                Debug.LogWarning("No Model for Set Card", this);
                return;
            }

            if (!hasSetCard)
            {
                var setCard = _dataManager.GetFromQueue(SetCardModelRecyclerKey, zone.position, zone.rotation, _speedDuelField.transform);
                _webRequest.RequestCardImageFromWeb(default, zone.name, cardId, true);
                InstantiatedModels.Add($"{zone.name}:{SetCard}", setCard);
            }

            _modelEventHandler.RaiseChangeVisibilityEvent(zone.name, false);
        }

        private void HandleFaceUpDefencePosition(GameObject model, Transform zone, bool hasSetCard, GameObject setCardModel, string cardId)
        {
            if (hasSetCard)
            {
                _webRequest.RequestCardImageFromWeb(EventNames.RevealSetMonster, zone.name, cardId, true);
                _modelEventHandler.RaiseChangeVisibilityEvent(zone.name, true);
                model.transform.position = setCardModel.transform.GetChild(0).GetChild(0).position;
                return;
            }

            var setCard = _dataManager.GetFromQueue(SetCardModelRecyclerKey, zone.position, zone.rotation, _speedDuelField.transform);
            _webRequest.RequestCardImageFromWeb(EventNames.RevealSetMonster, zone.name, cardId, true);
            model.transform.position = setCard.transform.position;
            InstantiatedModels.Add($"{zone.name}:{SetCard}", setCard);
            _modelEventHandler.RaiseChangeVisibilityEvent(zone.name, true);
        }

        private void OnRemovecardEventReceived(RemoveCardEvent removeCardEvent)
        {
            var modelExists = InstantiatedModels.TryGetValue(removeCardEvent.ZoneName, out var model);
            if (!modelExists)
            {
                RemoveSetCard(removeCardEvent);
                return;
            }

            var destructionParticles = _dataManager.GetFromQueue(
                ParticlesModelRecyclerKey, model.transform.position, model.transform.rotation, _speedDuelField.transform);

            _modelEventHandler.RaiseEventByEventName(EventNames.DestroyMonster, removeCardEvent.ZoneName);

            StartCoroutine(WaitToProceed(ParticlesModelRecyclerKey, destructionParticles));
            StartCoroutine(WaitToProceed(model.name, model));

            InstantiatedModels.Remove(removeCardEvent.ZoneName);

            var isModelSet = InstantiatedModels.TryGetValue($"{removeCardEvent.ZoneName}:{SetCard}", out var setCard);
            if (!isModelSet)
            {
                _modelEventHandler.RaiseEventByEventName(EventNames.DestroySetMonster, removeCardEvent.ZoneName);
                InstantiatedModels.Remove($"{removeCardEvent.ZoneName}:{SetCard}");
                StartCoroutine(WaitToProceed(SetCardModelRecyclerKey, setCard));

                _dataManager.AddToQueue(SetCardModelRecyclerKey, setCard);
            }
        }

        private void RemoveSetCard(RemoveCardEvent removeCardEvent)
        {
            var isCardSet = InstantiatedModels.TryGetValue($"{removeCardEvent.ZoneName}:{SetCard}", out var setCard);
            if (!isCardSet)
            {
                return;
            }

            _modelEventHandler.RaiseEventByEventName(EventNames.SetCardRemove, removeCardEvent.ZoneName);
            InstantiatedModels.Remove($"{removeCardEvent.ZoneName}:{SetCard}");
            StartCoroutine(WaitToProceed(SetCardModelRecyclerKey, setCard));
        }

        #endregion

        #region Coroutines

        private IEnumerator WaitToProceed(string key, GameObject model)
        {
            yield return _waitTime;
            _dataManager.AddToQueue(key.Split('(')[0], model);
        }

        #endregion
    }
}
