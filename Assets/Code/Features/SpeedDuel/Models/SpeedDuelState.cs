using System.Collections.Generic;

namespace Code.Features.SpeedDuel.Models
{
    public class SpeedDuelState
    {
        private PlayerState UserState { get; }
        private PlayerState OpponentState { get; }

        public SpeedDuelState(PlayerState userState, PlayerState opponentState)
        {
            UserState = userState;
            OpponentState = opponentState;
        }

        public SpeedDuelState CopyWith(PlayerState userState = null, PlayerState opponentState = null)
        {
            return new SpeedDuelState(
                userState ?? UserState,
                opponentState ?? OpponentState);
        }

        public IEnumerable<PlayerState> GetPlayerStates()
        {
            return new List<PlayerState>
            {
                UserState,
                OpponentState
            };
        }
    }
}