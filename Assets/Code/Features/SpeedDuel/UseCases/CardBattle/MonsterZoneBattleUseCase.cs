using Code.Core.Logger;
using Code.Core.SmartDuelServer.Entities.EventData.CardEvents;
using Code.Features.SpeedDuel.EventHandlers;
using Code.Features.SpeedDuel.EventHandlers.Entities;
using Code.Features.SpeedDuel.Models.Zones;

namespace Code.Features.SpeedDuel.UseCases.CardBattle
{
    public interface IMonsterZoneBattleUseCase
    {
        void Execute(SingleCardZone playerZone, SingleCardZone targetzone);
    }

    public class MonsterZoneBattleUseCase : IMonsterZoneBattleUseCase
    {
        private const string Tag = "MonsterZoneBattleUseCase";

        private readonly IModelEventHandler _modelEventHandler;
        private readonly ISetCardEventHandler _setCardEventHandler;
        private readonly IAppLogger _logger;

        public MonsterZoneBattleUseCase(
            IModelEventHandler modelEventHandler,
            ISetCardEventHandler setCardEventHandler,
            IAppLogger appLogger)
        {
            _modelEventHandler = modelEventHandler;
            _setCardEventHandler = setCardEventHandler;
            _logger = appLogger;
        }

        public void Execute(SingleCardZone playerZone, SingleCardZone targetzone)
        {
            ExecuteAttackEvent(playerZone, true);
            ExecuteAttackEvent(targetzone, false);
        }

        private void ExecuteAttackEvent(SingleCardZone zone, bool isAttackingMonster)
        {
            _logger.Log(Tag, $"Execute({zone.Card.YugiohCard.Id}, isAttackingMonster: {isAttackingMonster})");

            if (isAttackingMonster && zone.Card.CardPosition != CardPosition.FaceUp) return;

            var cardId = GetCardModelInstanceId(zone);
            var setCardId = GetSetCardInstanceId(zone);
            if (cardId.HasValue)
            {
                _modelEventHandler.Action(ModelEvent.Attack, cardId.Value, isAttackingMonster);
            }

            if (setCardId.HasValue && !isAttackingMonster)
            {
                _setCardEventHandler.Action(SetCardEvent.Hurt, setCardId.Value);
            }
        }

        private int? GetCardModelInstanceId(SingleCardZone zone)
        {
            if (zone.MonsterModel != null)
            {
                return zone.MonsterModel.GetInstanceID();
            }

            _logger.Log(Tag, $"{zone} does not have a valid monster for battle");
            return null;
        }

        private int? GetSetCardInstanceId(SingleCardZone zone)
        {
            if (zone.SetCardModel != null)
            {
                return zone.SetCardModel.GetInstanceID();
            }

            _logger.Log(Tag, $"{zone} does not have a set card");
            return null;
        }
    }
}