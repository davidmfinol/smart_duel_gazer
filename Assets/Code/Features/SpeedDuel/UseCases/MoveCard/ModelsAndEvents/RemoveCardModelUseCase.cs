using Code.Core.DataManager.GameObjects.Entities;
using Code.Core.DataManager.GameObjects.UseCases;
using Code.Core.Models.ModelEventsHandler;
using Code.Core.Models.ModelEventsHandler.Entities;
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

        public RemoveCardModelUseCase(
            IGetTransformedGameObjectUseCase getTransformedGameObjectUseCase,
            IRecycleGameObjectUseCase recycleGameObjectUseCase,
            ModelEventHandler modelEventHandler)
        {
            _getTransformedGameObjectUseCase = getTransformedGameObjectUseCase;
            _recycleGameObjectUseCase = recycleGameObjectUseCase;
            _modelEventHandler = modelEventHandler;
        }

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
                RemoveSetCard(oldCard, setCardModel);
            }

            // This clears the zone, apart from the zone type.
            return zone.CopyWith();
        }

        private void RemoveMonsterModel(PlayCard oldCard, GameObject monsterModel, GameObject setCardModel)
        {
            var destructionParticles = _getTransformedGameObjectUseCase.Execute(GameObjectKeys.ParticlesKey,
                monsterModel.transform.position, monsterModel.transform.rotation);

            _modelEventHandler.RaiseEventByEventName(ModelEvent.DestroyMonster, oldCard.ZoneType.ToString());

            _recycleGameObjectUseCase.Execute(GameObjectKeys.ParticlesKey, destructionParticles);
            _recycleGameObjectUseCase.Execute(oldCard.Id.ToString(), monsterModel);

            if (!setCardModel) return;

            _modelEventHandler.RaiseEventByEventName(ModelEvent.DestroySetMonster, oldCard.ZoneType.ToString());
            RemoveSetCard(oldCard, setCardModel);
        }

        private void RemoveSetCard(PlayCard oldCard, GameObject setCardModel)
        {
            _modelEventHandler.RaiseEventByEventName(ModelEvent.SetCardRemove, oldCard.ZoneType.ToString());
            _recycleGameObjectUseCase.Execute(GameObjectKeys.SetCardKey, setCardModel);
        }
    }
}