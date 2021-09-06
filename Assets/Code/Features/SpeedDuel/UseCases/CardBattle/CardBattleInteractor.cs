using Code.Core.SmartDuelServer.Entities.EventData.CardEvents;
using Code.Features.SpeedDuel.Models.Zones;

namespace Code.Features.SpeedDuel.UseCases.CardBattle
{
    public interface IMonsterBattleInteractor
    {
        void Execute(Zone attackZone, Zone targetZone);
    }

    public class MonsterBattleInteractor : IMonsterBattleInteractor
    {
        private readonly IMonsterBattleUseCase _monsterBattleUseCase;

        public MonsterBattleInteractor(
            IMonsterBattleUseCase monsterBattleUseCase)
        {
            _monsterBattleUseCase = monsterBattleUseCase;
        }

        public void Execute(Zone attackZone, Zone targetZone)
        {
            // The attack zone should always be a single card zone.
            if (attackZone is SingleCardZone singleCardAttackZone)
            {
                _monsterBattleUseCase.Execute(singleCardAttackZone, true);
            }

            switch (targetZone)
            {
                // If the target zone is a single card zone, then a monster battle occurs.
                case SingleCardZone singleCardTargetZone:
                    _monsterBattleUseCase.Execute(singleCardTargetZone, false);
                    break;
                // If the target zone is the hand zone, then a direct attack occurs.
                case MultiCardZone {ZoneType: ZoneType.Hand}:
                    // TODO: handle direct attack
                    break;
            }
        }
    }
}