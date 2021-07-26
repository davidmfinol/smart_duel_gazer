using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelComponentsManager;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler;
using AssemblyCSharp.Assets.Code.Core.Models.Interface.ModelEventsHandler.Entities;
using Code.Core.DataManager.Interface.GameObject.UseCases;
using Code.Core.SmartDuelServer.Interface.Entities.EventData.CardEvents;
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
        private const string SetCardKey = "SetCard";

        private readonly IDataManager _dataManager;
        private readonly IGetTransformedGameObjectUseCase _getTransformedGameObjectUseCase;
        private readonly IHandlePlayCardModelEventsUseCase _handlePlayCardModelEventsUseCase;
        private readonly ModelEventHandler _modelEventHandler;
        private readonly ModelComponentsManager.Factory _modelFactory;

        public PlayCardModelUseCase(
            IDataManager dataManager,
            IGetTransformedGameObjectUseCase getTransformedGameObjectUseCase,
            IHandlePlayCardModelEventsUseCase handlePlayCardModelEventsUseCase,
            ModelEventHandler modelEventHandler,
            ModelComponentsManager.Factory modelFactory)
        {
            _dataManager = dataManager;
            _getTransformedGameObjectUseCase = getTransformedGameObjectUseCase;
            _handlePlayCardModelEventsUseCase = handlePlayCardModelEventsUseCase;
            _modelEventHandler = modelEventHandler;
            _modelFactory = modelFactory;
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
            Debug.Log($"InstantiateModel(monsterModel: {monsterModel}, playMatZone: {playMatZone})");

            var instantiatedModel = monsterModel.IsClone()
                ? monsterModel
                : _modelFactory.Create(monsterModel).gameObject.transform.parent.gameObject;

            instantiatedModel.transform.SetParent(speedDuelField.transform);
            instantiatedModel.transform.SetPositionAndRotation(playMatZone.position, playMatZone.rotation);
            var model = instantiatedModel.transform.GetChild(0);
            _modelEventHandler.ActivateModel(model.GetInstanceID());

            return instantiatedModel;
        }

        private void HandleFaceUpPosition(PlayCard updatedCard, GameObject instantiatedMonsterModel, Transform playMatZone,
            GameObject currentSetCardModel, out GameObject newSetCardModel)
        {
            // If the monster is in face up position, there is no need for a set card,
            // so null is returned regardless of the current set card.
            newSetCardModel = null;

            var model = instantiatedMonsterModel.transform.GetChild(0);
            _modelEventHandler.RaiseEventByEventName(ModelEvent.SummonMonster, model.GetInstanceID());
            instantiatedMonsterModel.transform.position = playMatZone.position;

            if (!currentSetCardModel) return;

            _modelEventHandler.RaiseEventByEventName(ModelEvent.SetCardRemove, currentSetCardModel.GetInstanceID());
            currentSetCardModel.SetActive(false);
            _dataManager.SaveGameObject(SetCardKey, currentSetCardModel);
        }

        private void HandleFaceUpDefencePosition(PlayCard updatedCard, GameObject instantiatedMonsterModel, Transform playMatZone,
            GameObject currentSetCardModel, out GameObject newSetCardModel)
        {
            var setCard = currentSetCardModel
                ? currentSetCardModel
                : _getTransformedGameObjectUseCase.Execute(SetCardKey, playMatZone.position, playMatZone.rotation);

            newSetCardModel = setCard;

            // TODO: ask Subtle why/if the order here matters
            if (!currentSetCardModel)
            {
                _handlePlayCardModelEventsUseCase.Execute(ModelEvent.RevealSetMonster, updatedCard, setCard.GetInstanceID(), true);

                // This puts the model on top of the set card rather than clipping through it.
                instantiatedMonsterModel.transform.position = setCard.transform.GetChild(0).GetChild(0).position;

                var model = instantiatedMonsterModel.transform.GetChild(0);
                _modelEventHandler.RaiseChangeVisibilityEvent(model.GetInstanceID(), true);
            }
            else
            {
                _handlePlayCardModelEventsUseCase.Execute(ModelEvent.RevealSetMonster, updatedCard, setCard.GetInstanceID(), true);

                var model = instantiatedMonsterModel.transform.GetChild(0);
                _modelEventHandler.RaiseChangeVisibilityEvent(model.GetInstanceID(), true);

                // This puts the model on top of the set card rather than clipping through it.
                instantiatedMonsterModel.transform.position = setCard.transform.GetChild(0).GetChild(0).position;
            }
        }

        private void HandleFaceDownDefencePosition(PlayCard updatedCard, Transform playMatZone, GameObject currentSetCardModel,
            out GameObject newSetCardModel)
        {
            var setCard = currentSetCardModel
                ? currentSetCardModel
                : _getTransformedGameObjectUseCase.Execute(SetCardKey, playMatZone.position, playMatZone.rotation);

            newSetCardModel = setCard;

            if (!currentSetCardModel)
            {
                if (!setCard)
                {
                    Debug.LogWarning("The setCard queue is empty :(");
                    return;
                }

                _handlePlayCardModelEventsUseCase.Execute(default, updatedCard, setCard.GetInstanceID(), true);
            }

            _modelEventHandler.RaiseChangeVisibilityEvent(setCard.GetInstanceID(), false);
        }
    }
}