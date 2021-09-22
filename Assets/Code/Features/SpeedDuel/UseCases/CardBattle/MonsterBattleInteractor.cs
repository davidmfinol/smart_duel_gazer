using Code.Features.SpeedDuel.Models;
using Code.Features.SpeedDuel.Models.Zones;
using UnityEngine;

namespace Code.Features.SpeedDuel.UseCases.CardBattle
{
    public interface IMonsterBattleInteractor
    {
        void Execute(Zone playerZone, Zone targetZone, PlayerState targetState, GameObject speedDuelField);
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

        public void Execute(Zone playerZone, Zone targetZone, PlayerState targetState, GameObject speedDuelField)
        {
            if (!(playerZone is SingleCardZone playerSingleCardZone)) return;
            
            if (targetZone is MultiCardZone)
            {
                _directAttackUseCase.Execute(playerSingleCardZone, targetState, speedDuelField);
            }
            else if (targetZone is SingleCardZone targetSingleCardZone)
            {
                _monsterZoneBattleUseCase.Execute(playerSingleCardZone, targetSingleCardZone);
            }
        }
    }
}