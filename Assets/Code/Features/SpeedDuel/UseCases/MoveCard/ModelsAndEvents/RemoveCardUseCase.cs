using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler;
using AssemblyCSharp.Assets.Code.Core.Models.Interface.ModelEventsHandler.Entities;
using Code.Core.DataManager.Interface.GameObject.UseCases;
using Code.Features.SpeedDuel.Models;
using Code.Features.SpeedDuel.Models.Zones;
using UnityEngine;

namespace Code.Features.SpeedDuel.UseCases.MoveCard.ModelsAndEvents
{
    public interface IRemoveCardUseCase
    {
        Zone Execute(SingleCardZone zone, PlayCard oldCard);
    }

    public class RemoveCardUseCase : IRemoveCardUseCase
    {
        private const string SetCardKey = "SetCard";
        private const string ParticlesKey = "Particles";

        private readonly IGetTransformedGameObjectUseCase _getTransformedGameObjectUseCase;
        private readonly IRecycleGameObjectUseCase _recycleGameObjectUseCase;
        private readonly ModelEventHandler _modelEventHandler;

        public RemoveCardUseCase(
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
                RemoveSetCard(setCardModel);
            }

            // This clears the zone, apart from the zone type.
            return zone.CopyWith();
        }

        private void RemoveMonsterModel(PlayCard oldCard, GameObject monsterModel, GameObject setCardModel)
        {
            var destructionParticles = _getTransformedGameObjectUseCase.Execute(ParticlesKey, monsterModel.transform.position,
                monsterModel.transform.rotation);

            //instanceID is held by the child, the parent only acts as a container for the model
            var model = monsterModel.transform.GetChild(0);
            _modelEventHandler.RaiseEventByEventName(ModelEvent.DestroyMonster, model.GetInstanceID());

            _recycleGameObjectUseCase.Execute(ParticlesKey, destructionParticles);
            _recycleGameObjectUseCase.Execute(oldCard.Id.ToString(), monsterModel);

            if (!setCardModel) return;

            _modelEventHandler.RaiseEventByEventName(ModelEvent.DestroySetMonster, setCardModel.GetInstanceID());
            RemoveSetCard(setCardModel);
        }

        private void RemoveSetCard(GameObject setCardModel)
        {
            _modelEventHandler.RaiseEventByEventName(ModelEvent.SetCardRemove, setCardModel.GetInstanceID());
            _recycleGameObjectUseCase.Execute(SetCardKey, setCardModel);
        }
    }
}