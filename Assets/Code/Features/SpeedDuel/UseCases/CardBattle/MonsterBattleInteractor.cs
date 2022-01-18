using Code.Features.SpeedDuel.Models;
using Code.Features.SpeedDuel.Models.Zones;

namespace Code.Features.SpeedDuel.UseCases.CardBattle
{
    public interface IMonsterBattleInteractor
    {
        void Execute(Zone playerZone, Zone targetZone, PlayerState targetState);
    }

    public class MonsterBattleInteractor : IMonsterBattleInteractor
    {
        private readonly IMonsterZoneBattleUseCase _monsterZoneBattleUseCase;
        private readonly IDirectAttackUseCase _directAttackUseCase;

        public MonsterBattleInteractor(
            IMonsterZoneBattleUseCase monsterZoneBattleUseCase,
            IDirectAttackUseCase directAttackUseCase)
        {
            _monsterZoneBattleUseCase = monsterZoneBattleUseCase;
            _directAttackUseCase = directAttackUseCase;
        }

        public void Execute(Zone playerZone, Zone targetZone, PlayerState targetState)
        {
            if (!(playerZone is SingleCardZone playerSingleCardZone)) return;

            if (targetZone is MultiCardZone)
            {
                _directAttackUseCase.Execute(playerSingleCardZone, targetState);
            }
            else if (targetZone is SingleCardZone targetSingleCardZone)
            {
                _monsterZoneBattleUseCase.Execute(playerSingleCardZone, targetSingleCardZone, targetState.PlayMatZonesPath);
            }
        }
    }
}