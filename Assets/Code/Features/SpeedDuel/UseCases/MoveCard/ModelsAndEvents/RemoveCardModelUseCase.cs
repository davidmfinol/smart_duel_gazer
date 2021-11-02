using Code.Core.DataManager.GameObjects.Entities;
using Code.Core.DataManager.GameObjects.UseCases;
using Code.Features.SpeedDuel.EventHandlers;
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
        private readonly IModelEventHandler _modelEventHandler;
        private readonly ISetCardEventHandler _setCardEventHandler;

        #region Constructor

        public RemoveCardModelUseCase(
            IGetTransformedGameObjectUseCase getTransformedGameObjectUseCase,
            IRecycleGameObjectUseCase recycleGameObjectUseCase,
            IModelEventHandler modelEventHandler,
            ISetCardEventHandler setCardEventHandler)
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

            var modelID = monsterModel.GetInstanceID();
            _modelEventHandler.Remove(modelID);

            _recycleGameObjectUseCase.Execute(GameObjectKeys.ParticlesKey, destructionParticles);
            _recycleGameObjectUseCase.Execute(oldCard.Id.ToString(), monsterModel);

            if (!setCardModel) return;

            RemoveSetCard(setCardModel);
        }

        private void RemoveSetCard(GameObject setCardModel)
        {
            _setCardEventHandler.Remove(setCardModel.GetInstanceID());
            _recycleGameObjectUseCase.Execute(GameObjectKeys.SetCardKey, setCardModel);
        }
    }
}