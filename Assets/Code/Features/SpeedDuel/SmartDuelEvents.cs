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
    public class SmartDuelEvents : MonoBehaviour, ISmartDuelEventListener
    {
        private const string SetCard = "SetCard";
        private const string ParticlesKey = "Particles";
        private const string SetCardsKey = "SetCards";

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
            if (_speedDuelField == null)
            {
                _speedDuelField = GetComponent<PlacementEvents>().SpeedDuelField;
            }
            
            var zone = _speedDuelField.transform.Find($"Playmat/Zones/{playCardEvent.ZoneName}");
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

            GameObject instantiatedModel;
            if (!InstantiatedModels.ContainsKey(zone.name))
            {
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
            }

            InstantiatedModels.TryGetValue(zone.name, out var model);
            var hasSetCard = InstantiatedModels.TryGetValue(zone.name + SetCard, out var setCardModel);
            if (playCardEvent.CardPosition == "faceUp")
            {
                _modelEventHandler.RaiseEventByEventName(EventNames.SummonMonster, zone.name);
                model.transform.position = zone.position;

                if (hasSetCard)
                {
                    _modelEventHandler.RaiseEventByEventName(EventNames.SetCardRemove, zone.name);
                    InstantiatedModels.Remove(zone.name + SetCard);
                    _dataManager.AddToQueue(SetCardsKey, setCardModel);
                }
            }            
            else if (playCardEvent.CardPosition == "faceDownDefence")
            {
                if (_dataManager.GetCardModel(SetCard) == null)
                {
                    Debug.LogWarning("No Model for Set Card", this);
                    return;
                }

                if(!hasSetCard)
                {
                    var setCard = _dataManager.GetFromQueue(SetCardsKey, zone.position, zone.rotation, _speedDuelField.transform);
                    _webRequest.RequestCardImageFromWeb(default, zone.name, cardModel.name, true);
                    InstantiatedModels.Add(zone.name + SetCard, setCard);
                }

                _modelEventHandler.RaiseChangeVisibilityEvent(zone.name, false);
            }
            else if (playCardEvent.CardPosition == "faceUpDefence")
            {
                if (hasSetCard)
                {
                    _webRequest.RequestCardImageFromWeb(EventNames.RevealSetMonster, zone.name, cardModel.name, true);
                    _modelEventHandler.RaiseChangeVisibilityEvent(playCardEvent.ZoneName, true);
                    model.transform.position = setCardModel.transform.GetChild(0).GetChild(0).position;

                    return;
                }

                var setCard = _dataManager.GetFromQueue(SetCardsKey, zone.position, zone.rotation, _speedDuelField.transform);
                _webRequest.RequestCardImageFromWeb(EventNames.RevealSetMonster, zone.name, cardModel.name, true);
                model.transform.position = setCard.transform.position;
                InstantiatedModels.Add(playCardEvent.ZoneName + SetCard, setCard);
                _modelEventHandler.RaiseChangeVisibilityEvent(playCardEvent.ZoneName, true);      
            }
        }

        private void OnRemovecardEventReceived(RemoveCardEvent removeCardEvent)
        {
            var modelExists = InstantiatedModels.TryGetValue(removeCardEvent.ZoneName, out var model);
            if (!modelExists)
            {
                var modelSet = InstantiatedModels.TryGetValue(removeCardEvent.ZoneName + SetCard, out var setSpellTrap);
                if (!modelSet)
                {
                    return;
                }

                _modelEventHandler.RaiseEventByEventName(EventNames.SetCardRemove, removeCardEvent.ZoneName);
                InstantiatedModels.Remove(removeCardEvent.ZoneName + SetCard);
                StartCoroutine(WaitToProceed(SetCardsKey, setSpellTrap));
                return;
            }

            var destructionParticles = _dataManager.GetFromQueue(
                ParticlesKey, model.transform.position, model.transform.rotation, _speedDuelField.transform);
            
            _modelEventHandler.RaiseEventByEventName(EventNames.DestroyMonster, removeCardEvent.ZoneName);

            StartCoroutine(WaitToProceed(ParticlesKey, destructionParticles));
            StartCoroutine(WaitToProceed(model.name, model));

            InstantiatedModels.Remove(removeCardEvent.ZoneName);

            var modelIsSet = InstantiatedModels.TryGetValue(removeCardEvent.ZoneName + SetCard, out var setCard);
            if (!modelIsSet)
            {
                return;
            }

            _modelEventHandler.RaiseEventByEventName(EventNames.DestroySetMonster, removeCardEvent.ZoneName);
            InstantiatedModels.Remove(removeCardEvent.ZoneName + SetCard);
            StartCoroutine(WaitToProceed(SetCardsKey, setCard));

            _dataManager.AddToQueue(SetCardsKey, setCard);
        }

        private void SummonCardWithoutModel(PlayCardEvent playCardEvent, Transform zone)
        {
            if (!InstantiatedModels.TryGetValue(zone.name + SetCard, out var _)) 
            {
                var setCard = _dataManager.GetFromQueue(SetCardsKey, zone.position, zone.rotation, _speedDuelField.transform);
                InstantiatedModels.Add(zone.name + SetCard, setCard);
            }

            //These aren't tested for monster cards, only spells and traps as no monster cards without models exist yet
            if (playCardEvent.CardPosition == "faceUp")
            {
                _webRequest.RequestCardImageFromWeb(EventNames.SpellTrapActivate, zone.name, playCardEvent.CardId, false);
            }
            else if (playCardEvent.CardPosition == "faceDown")
            {
                _webRequest.RequestCardImageFromWeb(default, zone.name, playCardEvent.CardId, false);
            }
            else if (playCardEvent.CardPosition == "faceUpDefence")
            {
                _webRequest.RequestCardImageFromWeb(EventNames.RevealSetMonster, zone.name, playCardEvent.CardId, true);
            }
            else if (playCardEvent.CardPosition == "faceDownDefence")
            {
                _webRequest.RequestCardImageFromWeb(default, zone.name, playCardEvent.CardId, true);
            }
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
