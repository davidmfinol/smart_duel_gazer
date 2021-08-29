using Code.Features.SpeedDuel.Models;

namespace Code.Features.SpeedDuel.UseCases.CardBattle
{
    public interface IMonsterBattleInteractor
    {
        void Execute(PlayerState playerState, PlayCard playerCard, PlayerState targetState, PlayCard targetCard);
    }

    public class MonsterBattleInteractor : IMonsterBattleInteractor
    {
        private readonly IMonsterBattleUseCase _monsterBattleUseCase;
        
        public MonsterBattleInteractor(
            IMonsterBattleUseCase monsterBattleUseCase)
        {
            _monsterBattleUseCase = monsterBattleUseCase;
        }
        
        public void Execute(PlayerState playerState, PlayCard playerCard, PlayerState targetState, PlayCard targetCard)
        {
            _monsterBattleUseCase.Execute(playerState, playerCard, true);
            _monsterBattleUseCase.Execute(targetState, targetCard, false);
        }        
    }
}