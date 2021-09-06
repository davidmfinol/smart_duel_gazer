using Code.Core.Logger;
using Code.Features.SpeedDuel.EventHandlers;
using Code.Features.SpeedDuel.EventHandlers.Entities;
using Code.Features.SpeedDuel.Models;
using Code.Features.SpeedDuel.Models.Zones;

namespace Code.Features.SpeedDuel.UseCases.CardBattle
{
    public interface IMonsterBattleUseCase
    {
        void Execute(SingleCardZone zone, bool isAttackingMonster);
    }

    public class MonsterBattleUseCase : IMonsterBattleUseCase
    {
        private const string Tag = "MonsterBattleUseCase";

        private readonly ModelEventHandler _modelEventHandler;
        private readonly IAppLogger _logger;

        public MonsterBattleUseCase(
            ModelEventHandler modelEventHandler,
            IAppLogger appLogger)
        {
            _modelEventHandler = modelEventHandler;
            _logger = appLogger;
        }

        public void Execute(SingleCardZone zone, bool isAttackingMonster)
        {
            _logger.Log(Tag, $"Execute({zone.ZoneType}, isAttackingMonster: {isAttackingMonster})");

            var monsterInstanceId = GetMonsterInstanceId(zone);
            if (monsterInstanceId.HasValue)
            {
                _modelEventHandler.Action(ModelEvent.Attack, monsterInstanceId.Value, isAttackingMonster);
            }
        }

        private int? GetMonsterInstanceId(SingleCardZone zone)
        {
            if (zone.MonsterModel != null)
            {
                return zone.MonsterModel.GetInstanceID();
            }
            
            _logger.Log(Tag, $"{zone} does not have a valid monster for battle");
            return null;
        }
    }
}