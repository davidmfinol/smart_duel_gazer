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
        private const float RemoveCardDurationInSeconds = 7;

        private const string PlaymatZonesPath = "Playmat/Zones";
        private const string SetCard = "SetCard";

        private const string ParticlesModelRecyclerKey = "Particles";
        private const string SetCardModelRecyclerKey = "SetCards";

        private ISmartDuelServer _smartDuelServer;
        private IDataManager _dataManager;
        private IApiWebRequest _webRequest;
        private ModelEventHandler _modelEventHandler;
        private ModelComponentsManager.Factory _modelFactory;

        private GameObject _speedDuelField;

        #region Properties

        // TODO: work more OO: create PlayMat/Field entity with zones which contain models
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
            FetchSpeedDuelField();

            if (smartDuelEvent is PlayCardEvent playCardEvent)
            {
                OnPlayCardEventReceived(playCardEvent);
            }
            else if (smartDuelEvent is RemoveCardEvent removeCardEvent)
            {
                OnRemoveCardEventReceived(removeCardEvent);
            }
        }

        private void FetchSpeedDuelField()
        {
            if (_speedDuelField == null)
            {
                _speedDuelField = GetComponent<PlacementEvents>().SpeedDuelField;
            }
        }

        #region PlayCardEvent

        private void OnPlayCardEventReceived(PlayCardEvent playCardEvent)
        {
            var zone = _speedDuelField.transform.Find($"{PlaymatZonesPath}/{playCardEvent.ZoneName}");
            if (zone == null)
            {
                return;
            }

            var cardModel = _dataManager.GetCardModel(playCardEvent.CardId);
            if (cardModel == null)
            {
                // TODO: create a function which returns either an instantiated 3D model of the monster
                // or an instantiated model of the card, and then handle the position
                PlayCardWithoutModel(playCardEvent, zone);
                return;
            }

            PlayCardWithModel(playCardEvent, zone, cardModel);
        }

        private void PlayCardWithoutModel(PlayCardEvent playCardEvent, Transform zone)
        {
            if (!InstantiatedModels.TryGetValue($"{zone.name}:{SetCard}", out var _))
            {
                var setCard = _dataManager.GetFromQueue(SetCardModelRecyclerKey, zone.position, zone.rotation, _speedDuelField.transform);
                InstantiatedModels.Add($"{zone.name}:{SetCard}", setCard);
            }

            // TODO: Test with monster for which there's no 3D model
            // TODO: Create data manager for handling most of this logic
            switch (playCardEvent.CardPosition)
            {
                case CardPosition.FaceUp:
                    _webRequest.RequestCardImageFromWeb(EventNames.SpellTrapActivate, zone.name, playCardEvent.CardId, false);
                    break;
                case CardPosition.FaceDown:
                    _webRequest.RequestCardImageFromWeb(default, zone.name, playCardEvent.CardId, false);
                    break;
                case CardPosition.FaceUpDefence:
                    _webRequest.RequestCardImageFromWeb(EventNames.RevealSetMonster, zone.name, playCardEvent.CardId, true);
                    break;
                case CardPosition.FaceDownDefence:
                    _webRequest.RequestCardImageFromWeb(default, zone.name, playCardEvent.CardId, true);
                    break;
            }
        }

        private void PlayCardWithModel(PlayCardEvent playCardEvent, Transform zone, GameObject cardModel)
        {
            var instantiatedModel = GetInstantiatedModel(cardModel, zone);
            var hasSetCard = InstantiatedModels.TryGetValue($"{zone.name}:{SetCard}", out var setCardModel);

            switch(playCardEvent.CardPosition)
            {
                case CardPosition.FaceUp:
                    HandleFaceUpPosition(instantiatedModel, zone, hasSetCard, setCardModel);
                    break;
                case CardPosition.FaceDown:
                    HandleFaceUpDefencePosition(instantiatedModel, zone, hasSetCard, setCardModel, instantiatedModel.name);
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
            GameObject instantiatedModel;
            if (_dataManager.IsModelRecyclable(cardModel.name))
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

        private void HandleFaceUpPosition(GameObject instantiatedModel, Transform zone, bool hasSetCard, GameObject setCardModel)
        {
            _modelEventHandler.RaiseEventByEventName(EventNames.SummonMonster, zone.name);
            instantiatedModel.transform.position = zone.position;

            if (hasSetCard)
            {
                _modelEventHandler.RaiseEventByEventName(EventNames.SetCardRemove, zone.name);
                InstantiatedModels.Remove($"{zone.name}:{SetCard}");
                _dataManager.AddToQueue(SetCardModelRecyclerKey, setCardModel);
            }
        }

        private void HandleFaceDownDefencePosition(Transform zone, bool hasSetCard, string cardId)
        {
            // TODO: create special function for the set card model, and remove the resource from the monster resources folder
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

        #endregion

        #region RemoveCardEvent

        private void OnRemoveCardEventReceived(RemoveCardEvent removeCardEvent)
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

            StartCoroutine(RemoveCard(ParticlesModelRecyclerKey, destructionParticles));
            StartCoroutine(RemoveCard(model.name, model));

            InstantiatedModels.Remove(removeCardEvent.ZoneName);

            var isModelSet = InstantiatedModels.TryGetValue($"{removeCardEvent.ZoneName}:{SetCard}", out var setCard);
            if (!isModelSet)
            {
                _modelEventHandler.RaiseEventByEventName(EventNames.DestroySetMonster, removeCardEvent.ZoneName);
                InstantiatedModels.Remove($"{removeCardEvent.ZoneName}:{SetCard}");
                StartCoroutine(RemoveCard(SetCardModelRecyclerKey, setCard));

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
            StartCoroutine(RemoveCard(SetCardModelRecyclerKey, setCard));
        }

        private IEnumerator RemoveCard(string key, GameObject model)
        {
            yield return new WaitForSeconds(RemoveCardDurationInSeconds);
            _dataManager.AddToQueue(key.Split('(')[0], model);
        }

        #endregion

        #endregion
    }
}
