using Code.Core.DataManager;
using Code.Core.DataManager.GameObjects.Entities;
using Code.Core.DataManager.GameObjects.UseCases;
using Code.Core.General.Extensions;
using Code.Core.Logger;
using Code.Core.Models.ModelComponentsManager;
using Code.Core.Models.ModelEventsHandler;
using Code.Core.Models.ModelEventsHandler.Entities;
using Code.Core.SmartDuelServer.Entities.EventData.CardEvents;
using Code.Features.SpeedDuel.Models;
using Code.Features.SpeedDuel.Models.Zones;
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
        private readonly ModelComponentsManager.Factory _modelFactory;
        private readonly IAppLogger _logger;

        public PlayCardModelUseCase(
            IDataManager dataManager,
            IGetTransformedGameObjectUseCase getTransformedGameObjectUseCase,
            IHandlePlayCardModelEventsUseCase handlePlayCardModelEventsUseCase,
            ModelEventHandler modelEventHandler,
            ModelComponentsManager.Factory modelFactory,
            IAppLogger logger)
        {
            _dataManager = dataManager;
            _getTransformedGameObjectUseCase = getTransformedGameObjectUseCase;
            _handlePlayCardModelEventsUseCase = handlePlayCardModelEventsUseCase;
            _modelEventHandler = modelEventHandler;
            _modelFactory = modelFactory;
            _logger = logger;
        }

        public Zone Execute(SingleCardZone zone, PlayCard updatedCard, Transform playMatZone, GameObject monsterModel,
            GameObject speedDuelField)
        {
            var instantiatedMonsterModel = GetInstantiatedModel(zone, updatedCard, playMatZone, monsterModel, speedDuelField);
            var currentSetCardModel = zone.SetCardModel;

            GameObject newSetCardModel = null;
            switch (updatedCard.CardPosition)
            {
                case CardPosition.FaceUp:
                    HandleFaceUpPosition(updatedCard, instantiatedMonsterModel, playMatZone, currentSetCardModel,
                        out newSetCardModel);
                    break;
                case CardPosition.FaceUpDefence:
                    HandleFaceUpDefencePosition(updatedCard, instantiatedMonsterModel, playMatZone, currentSetCardModel,
                        out newSetCardModel);
                    break;
                case CardPosition.FaceDownDefence:
                    HandleFaceDownDefencePosition(updatedCard, playMatZone, currentSetCardModel,
                        out newSetCardModel);
                    break;
            }

            return zone.CopyWith(updatedCard, newSetCardModel, instantiatedMonsterModel);
        }

        private GameObject GetInstantiatedModel(SingleCardZone zone, PlayCard updatedCard, Transform playMatZone,
            GameObject monsterModel, GameObject speedDuelField)
        {
            return zone.MonsterModel
                ? zone.MonsterModel
                : InstantiateModel(updatedCard, playMatZone, monsterModel, speedDuelField);
        }

        private GameObject InstantiateModel(PlayCard updatedCard, Transform playMatZone, GameObject monsterModel,
            GameObject speedDuelField)
        {
            _logger.Log(Tag, $"InstantiateModel(monsterModel: {monsterModel}, playMatZone: {playMatZone})");

            var instantiatedModel = monsterModel.IsClone()
                ? monsterModel
                : _modelFactory.Create(monsterModel).gameObject.transform.parent.gameObject;

            instantiatedModel.transform.SetParent(speedDuelField.transform);
            instantiatedModel.transform.SetPositionAndRotation(playMatZone.position, playMatZone.rotation);
            _modelEventHandler.ActivateModel(updatedCard.ZoneType.ToString());

            return instantiatedModel;
        }

        private void HandleFaceUpPosition(PlayCard updatedCard, GameObject instantiatedMonsterModel, Transform playMatZone,
            GameObject currentSetCardModel, out GameObject newSetCardModel)
        {
            // If the monster is in face up position, there is no need for a set card,
            // so null is returned regardless of the current set card.
            newSetCardModel = null;

            _modelEventHandler.RaiseEventByEventName(ModelEvent.SummonMonster, updatedCard.ZoneType.ToString());
            instantiatedMonsterModel.transform.position = playMatZone.position;

            if (!currentSetCardModel) return;

            _modelEventHandler.RaiseEventByEventName(ModelEvent.SetCardRemove, updatedCard.ZoneType.ToString());
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

            // TODO: ask Subtle why/if the order here matters
            if (!currentSetCardModel)
            {
                _handlePlayCardModelEventsUseCase.Execute(ModelEvent.RevealSetMonster, updatedCard, true);

                // This puts the model on top of the set card rather than clipping through it.
                instantiatedMonsterModel.transform.position = setCard.transform.GetChild(0).GetChild(0).position;

                _modelEventHandler.RaiseChangeVisibilityEvent(updatedCard.ZoneType.ToString(), true);
            }
            else
            {
                _handlePlayCardModelEventsUseCase.Execute(ModelEvent.RevealSetMonster, updatedCard, true);

                _modelEventHandler.RaiseChangeVisibilityEvent(updatedCard.ZoneType.ToString(), true);

                // This puts the model on top of the set card rather than clipping through it.
                instantiatedMonsterModel.transform.position = setCard.transform.GetChild(0).GetChild(0).position;
            }
        }

        private void HandleFaceDownDefencePosition(PlayCard updatedCard, Transform playMatZone, GameObject currentSetCardModel,
            out GameObject newSetCardModel)
        {
            var setCard = currentSetCardModel
                ? currentSetCardModel
                : _getTransformedGameObjectUseCase.Execute(GameObjectKeys.SetCardKey, playMatZone.position, playMatZone.rotation);

            newSetCardModel = setCard;

            if (!currentSetCardModel)
            {
                if (!setCard)
                {
                    _logger.Warning(Tag, "The setCard queue is empty :(");
                    return;
                }

                _handlePlayCardModelEventsUseCase.Execute(default, updatedCard, true);
            }

            _modelEventHandler.RaiseChangeVisibilityEvent(updatedCard.ZoneType.ToString(), false);
        }
    }
}