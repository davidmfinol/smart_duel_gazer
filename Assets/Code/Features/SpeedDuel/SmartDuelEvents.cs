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
        private static readonly string SET_CARD = "SetCard";
        private static readonly string PLAYMAT_ZONES = "Playmat/Zones/";

        private const string _keyParticles = "Particles";
        private const string _keySetCard = "SetCards";

        private ISmartDuelServer _smartDuelServer;
        private IDataManager _dataManager;
        private IApiWebRequest _webRequest;
        private ModelEventHandler _modelEventHandler;
        private ModelComponentsManager.Factory _modelFactory;
        
        private GameObject SpeedDuelField;
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

        #endregion

        #region Lifecycle

        private void OnDestroy()
        {
            _smartDuelServer?.Dispose();
        }

        #endregion

        #region Smart duel events

        private void ConnectToServer()
        {
            _smartDuelServer.Connect(this);
        }

        public void onSmartDuelEventReceived(SmartDuelEvent smartDuelEvent)
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
            if (SpeedDuelField == null)
            {
                SpeedDuelField = GetComponent<PlacementEvents>().SpeedDuelField;
            }
            
            var zone = SpeedDuelField.transform.Find(PLAYMAT_ZONES + playCardEvent.ZoneName);
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
                        cardModel.name, zone.position, zone.rotation, SpeedDuelField.transform);
                }
                else
                {
                    instantiatedModel = _modelFactory.Create(cardModel).gameObject.transform.parent.gameObject;
                    instantiatedModel.transform.SetParent(SpeedDuelField.transform);
                    instantiatedModel.transform.SetPositionAndRotation(zone.position, zone.rotation);
                }

                _modelEventHandler.ActivateModel(zone.name);
                InstantiatedModels.Add(zone.name, instantiatedModel);
            }

            InstantiatedModels.TryGetValue(zone.name, out var model);
            var hasSetCard = InstantiatedModels.TryGetValue(zone.name + SET_CARD, out var setCardModel);
            if (playCardEvent.CardPosition == "faceUp")
            {
                _modelEventHandler.RaiseEventByEventName(EventNames.SummonMonster, zone.name);
                model.transform.position = zone.position;

                if (hasSetCard)
                {
                    _modelEventHandler.RaiseEventByEventName(EventNames.SetCardRemove, zone.name);
                    InstantiatedModels.Remove(zone.name + SET_CARD);
                    _dataManager.AddToQueue(_keySetCard, setCardModel);
                }
            }            
            else if (playCardEvent.CardPosition == "faceDownDefence")
            {
                if (_dataManager.GetCardModel(SET_CARD) == null)
                {
                    Debug.LogWarning("No Model for Set Card", this);
                    return;
                }

                if(!hasSetCard)
                {
                    var setCard = _dataManager.GetFromQueue(_keySetCard, zone.position, zone.rotation, SpeedDuelField.transform);
                    _webRequest.RequestCardImageFromWeb(default, zone.name, cardModel.name, true);
                    InstantiatedModels.Add(zone.name + SET_CARD, setCard);
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

                var setCard = _dataManager.GetFromQueue(_keySetCard, zone.position, zone.rotation, SpeedDuelField.transform);
                _webRequest.RequestCardImageFromWeb(EventNames.RevealSetMonster, zone.name, cardModel.name, true);
                model.transform.position = setCard.transform.position;
                InstantiatedModels.Add(playCardEvent.ZoneName + SET_CARD, setCard);
                _modelEventHandler.RaiseChangeVisibilityEvent(playCardEvent.ZoneName, true);      
            }
        }

        private void OnRemovecardEventReceived(RemoveCardEvent removeCardEvent)
        {
            var modelExists = InstantiatedModels.TryGetValue(removeCardEvent.ZoneName, out var model);
            if (!modelExists)
            {
                var modelSet = InstantiatedModels.TryGetValue(removeCardEvent.ZoneName + SET_CARD, out var setSpellTrap);
                if (!modelSet)
                {
                    return;
                }

                _modelEventHandler.RaiseEventByEventName(EventNames.SetCardRemove, removeCardEvent.ZoneName);
                InstantiatedModels.Remove(removeCardEvent.ZoneName + SET_CARD);
                StartCoroutine(WaitToProceed(_keySetCard, setSpellTrap));
                return;
            }

            var destructionParticles = _dataManager.GetFromQueue(
                _keyParticles, model.transform.position, model.transform.rotation, SpeedDuelField.transform);
            
            _modelEventHandler.RaiseEventByEventName(EventNames.DestroyMonster, removeCardEvent.ZoneName);

            StartCoroutine(WaitToProceed(_keyParticles, destructionParticles));
            StartCoroutine(WaitToProceed(model.name, model));

            InstantiatedModels.Remove(removeCardEvent.ZoneName);

            var modelIsSet = InstantiatedModels.TryGetValue(removeCardEvent.ZoneName + SET_CARD, out var setCard);
            if (!modelIsSet)
            {
                return;
            }

            _modelEventHandler.RaiseEventByEventName(EventNames.DestroySetMonster, removeCardEvent.ZoneName);
            InstantiatedModels.Remove(removeCardEvent.ZoneName + SET_CARD);
            StartCoroutine(WaitToProceed(_keySetCard, setCard));

            _dataManager.AddToQueue(_keySetCard, setCard);
        }

        private void SummonCardWithoutModel(PlayCardEvent playCardEvent, Transform zone)
        {
            if (!InstantiatedModels.TryGetValue(zone.name + SET_CARD, out var _)) 
            {
                var setCard = _dataManager.GetFromQueue(_keySetCard, zone.position, zone.rotation, SpeedDuelField.transform);
                InstantiatedModels.Add(zone.name + SET_CARD, setCard);
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
