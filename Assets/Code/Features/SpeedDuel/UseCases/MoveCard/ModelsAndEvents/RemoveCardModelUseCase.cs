using Code.Core.DataManager.GameObjects.Entities;
using Code.Core.DataManager.GameObjects.UseCases;
using Code.Core.General.Extensions;
using Code.Features.SpeedDuel.EventHandlers;
using Code.Features.SpeedDuel.EventHandlers.Entities;
using Code.Features.SpeedDuel.Models;
using Code.Features.SpeedDuel.Models.Zones;
using UnityEngine;

namespace Code.Features.SpeedDuel.UseCases.MoveCard.ModelsAndEvents
{
    public interface IRemoveCardModelUseCase
    {
        Zone Execute(SingleCardZone zone, PlayCard oldCard);
    }

    public class RemoveCardModelUseCase : IRemoveCardModelUseCase
    {
        private readonly IGetTransformedGameObjectUseCase _getTransformedGameObjectUseCase;
        private readonly IRecycleGameObjectUseCase _recycleGameObjectUseCase;
        private readonly ModelEventHandler _modelEventHandler;
        private readonly SetCardEventHandler _setCardEventHandler;

        #region Constructor

        public RemoveCardModelUseCase(
            IGetTransformedGameObjectUseCase getTransformedGameObjectUseCase,
            IRecycleGameObjectUseCase recycleGameObjectUseCase,
            ModelEventHandler modelEventHandler,
            SetCardEventHandler setCardEventHandler)
        {
            _getTransformedGameObjectUseCase = getTransformedGameObjectUseCase;
            _recycleGameObjectUseCase = recycleGameObjectUseCase;
            _modelEventHandler = modelEventHandler;
            _setCardEventHandler = setCardEventHandler;
        }

        #endregion

        public Zone Execute(SingleCardZone zone, PlayCard oldCard)
        {
            var monsterModel = zone.MonsterModel;
            var setCardModel = zone.SetCardModel;

            if (monsterModel)
            {
                RemoveMonsterModel(oldCard, monsterModel, setCardModel);
            }
            else if (setCardModel)
            {
                RemoveSetCard(setCardModel);
            }

            // This clears the zone, apart from the zone type.
            return zone.CopyWith();
        }

        private void RemoveMonsterModel(PlayCard oldCard, GameObject monsterModel, GameObject setCardModel)
        {
            var destructionParticles = _getTransformedGameObjectUseCase.Execute(GameObjectKeys.ParticlesKey,
                monsterModel.transform.position, monsterModel.transform.rotation);

            var modelID = monsterModel.GetInstanceIDForModel();
            _modelEventHandler.RaiseEventByEventName(ModelEvent.DestroyMonster, modelID);

            _recycleGameObjectUseCase.Execute(GameObjectKeys.ParticlesKey, destructionParticles);
            _recycleGameObjectUseCase.Execute(oldCard.Id.ToString(), monsterModel);

            if (!setCardModel) return;

            _setCardEventHandler.RaiseEventByEventName(SetCardEvent.DestroySetMonster, setCardModel.GetInstanceIDForSetCard());
            RemoveSetCard(setCardModel);
        }

        private void RemoveSetCard(GameObject setCardModel)
        {
            _setCardEventHandler.RaiseEventByEventName(SetCardEvent.SetCardRemove, setCardModel.GetInstanceIDForSetCard());
            _recycleGameObjectUseCase.Execute(GameObjectKeys.SetCardKey, setCardModel);
        }
    }
}