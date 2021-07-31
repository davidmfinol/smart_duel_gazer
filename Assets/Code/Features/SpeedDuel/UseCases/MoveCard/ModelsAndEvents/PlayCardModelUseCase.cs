using Code.Core.DataManager;
using Code.Core.DataManager.GameObjects.Entities;
using Code.Core.DataManager.GameObjects.UseCases;
using Code.Core.General.Extensions;
using Code.Core.Logger;
using Code.Core.SmartDuelServer.Entities.EventData.CardEvents;
using Code.Features.SpeedDuel.EventHandlers;
using Code.Features.SpeedDuel.EventHandlers.Entities;
using Code.Features.SpeedDuel.Models;
using Code.Features.SpeedDuel.Models.Zones;
using Code.Features.SpeedDuel.PrefabManager.ModelComponentsManager;
using UnityEngine;

namespace Code.Features.SpeedDuel.UseCases.MoveCard.ModelsAndEvents
{
    public interface IPlayCardModelUseCase
    {
        Zone Execute(SingleCardZone zone, PlayCard updatedCard, Transform playMatZone, GameObject monsterModel,
            GameObject speedDuelField);
    }

    public class PlayCardModelUseCase : IPlayCardModelUseCase
    {
        private const string Tag = "PlayCardModelUseCase";

        private readonly IDataManager _dataManager;
        private readonly IGetTransformedGameObjectUseCase _getTransformedGameObjectUseCase;
        private readonly IHandlePlayCardModelEventsUseCase _handlePlayCardModelEventsUseCase;
        private readonly ModelEventHandler _modelEventHandler;
        private readonly SetCardEventHandler _setCardEventHandler;
        private readonly ModelComponentsManager.Factory _modelFactory;
        private readonly IAppLogger _logger;

        #region Constructor

        public PlayCardModelUseCase(
            IDataManager dataManager,
            IGetTransformedGameObjectUseCase getTransformedGameObjectUseCase,
            IHandlePlayCardModelEventsUseCase handlePlayCardModelEventsUseCase,
            ModelEventHandler modelEventHandler,
            SetCardEventHandler setCardEventHandler,
            ModelComponentsManager.Factory modelFactory,
            IAppLogger logger)
        {
            _dataManager = dataManager;
            _getTransformedGameObjectUseCase = getTransformedGameObjectUseCase;
            _handlePlayCardModelEventsUseCase = handlePlayCardModelEventsUseCase;
            _modelEventHandler = modelEventHandler;
            _setCardEventHandler = setCardEventHandler;
            _modelFactory = modelFactory;
            _logger = logger;
        }

        #endregion

        public Zone Execute(SingleCardZone zone, PlayCard updatedCard, Transform playMatZone, GameObject monsterModel,
            GameObject speedDuelField)
        {
            var instantiatedMonsterModel = GetInstantiatedModel(zone, playMatZone, monsterModel, speedDuelField);
            var currentSetCardModel = zone.SetCardModel;

            GameObject newSetCardModel = null;
            switch (updatedCard.CardPosition)
            {
                case CardPosition.FaceUp:
                    HandleFaceUpPosition(instantiatedMonsterModel, playMatZone, currentSetCardModel,
                        out newSetCardModel);
                    break;
                case CardPosition.FaceUpDefence:
                    HandleFaceUpDefencePosition(updatedCard, instantiatedMonsterModel, playMatZone, currentSetCardModel,
                        out newSetCardModel);
                    break;
                case CardPosition.FaceDownDefence:
                    HandleFaceDownDefencePosition(updatedCard, playMatZone, currentSetCardModel, instantiatedMonsterModel,
                        out newSetCardModel);
                    break;
            }

            return zone.CopyWith(updatedCard, newSetCardModel, instantiatedMonsterModel);
        }

        private GameObject GetInstantiatedModel(SingleCardZone zone, Transform playMatZone, GameObject monsterModel, 
            GameObject speedDuelField)
        {
            return zone.MonsterModel
                ? zone.MonsterModel
                : InstantiateModel(playMatZone, monsterModel, speedDuelField);
        }

        private GameObject InstantiateModel(Transform playMatZone, GameObject monsterModel, GameObject speedDuelField)
        {
            _logger.Log(Tag, $"InstantiateModel(monsterModel: {monsterModel}, playMatZone: {playMatZone})");

            var instantiatedModel = monsterModel.IsClone()
                ? monsterModel
                : _modelFactory.Create(monsterModel).gameObject.transform.parent.gameObject;

            instantiatedModel.transform.SetParent(speedDuelField.transform);
            instantiatedModel.transform.SetPositionAndRotation(playMatZone.position, playMatZone.rotation);

            var modelID = instantiatedModel.GetInstanceIDForModel();
            _modelEventHandler.Activate(modelID);

            return instantiatedModel;
        }

        private void HandleFaceUpPosition(GameObject instantiatedMonsterModel, Transform playMatZone, GameObject currentSetCardModel, 
            out GameObject newSetCardModel)
        {
            // If the monster is in face up position, there is no need for a set card,
            // so null is returned regardless of the current set card.
            newSetCardModel = null;

            var modelID = instantiatedMonsterModel.GetInstanceIDForModel();
            _modelEventHandler.RaiseEventByEventName(ModelEvent.SummonMonster, modelID);
            instantiatedMonsterModel.transform.position = playMatZone.position;

            if (!currentSetCardModel) return;

            _setCardEventHandler.RaiseEventByEventName(SetCardEvent.SetCardRemove, currentSetCardModel.GetInstanceID());
            currentSetCardModel.SetActive(false);
            _dataManager.SaveGameObject(GameObjectKeys.SetCardKey, currentSetCardModel);
        }

        private void HandleFaceUpDefencePosition(PlayCard updatedCard, GameObject instantiatedMonsterModel, Transform playMatZone,
            GameObject currentSetCardModel, out GameObject newSetCardModel)
        {
            var setCard = currentSetCardModel
                ? currentSetCardModel
                : _getTransformedGameObjectUseCase.Execute(GameObjectKeys.SetCardKey, playMatZone.position, playMatZone.rotation);

            newSetCardModel = setCard;

            // TODO: clean up this function
            if (!currentSetCardModel)
            {
                _handlePlayCardModelEventsUseCase.Execute(SetCardEvent.ShowSetCard, updatedCard, setCard.GetInstanceIDForSetCard(), true);

                instantiatedMonsterModel.PlaceOnTopOfSetCard(setCard);

                var modelID = instantiatedMonsterModel.GetInstanceIDForModel();
                _modelEventHandler.RaiseChangeVisibilityEvent(modelID, true);
                _modelEventHandler.RaiseEventByEventName(ModelEvent.RevealSetMonster, modelID);
            }
            else
            {
                _handlePlayCardModelEventsUseCase.Execute(SetCardEvent.ShowSetCard, updatedCard, setCard.GetInstanceIDForSetCard(), true);

                var modelID = instantiatedMonsterModel.GetInstanceIDForModel();
                _modelEventHandler.RaiseChangeVisibilityEvent(modelID, true);
                _modelEventHandler.RaiseEventByEventName(ModelEvent.RevealSetMonster, modelID);

                instantiatedMonsterModel.PlaceOnTopOfSetCard(setCard);
            }
        }

        private void HandleFaceDownDefencePosition(PlayCard updatedCard, Transform playMatZone, GameObject currentSetCardModel,
            GameObject instantiatedMonsterModel, out GameObject newSetCardModel)
        {
            var setCard = currentSetCardModel
                ? currentSetCardModel
                : _getTransformedGameObjectUseCase.Execute(GameObjectKeys.SetCardKey, playMatZone.position, playMatZone.rotation);

            var modelID = instantiatedMonsterModel.GetInstanceIDForModel();
            var setCardID = setCard.GetInstanceIDForSetCard();

            newSetCardModel = setCard;

            if (!currentSetCardModel)
            {
                if (!setCard)
                {
                    //TODO: Create function to instantiate new SetCards
                    _logger.Warning(Tag, "The setCard queue is empty :(");
                    return;
                }

                _handlePlayCardModelEventsUseCase.Execute(default, updatedCard, setCardID, true);
            }            

            _modelEventHandler.RaiseChangeVisibilityEvent(modelID, false);
            _setCardEventHandler.RaiseEventByEventName(SetCardEvent.HideSetMonster, setCardID);
        }
    }
}