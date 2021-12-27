using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

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
        
        [CanBeNull]
        public PlayCard GetPlayCard(string duelistId, int cardId, int copyNumber)
        {
            return GetPlayerStates()
                .Select(playerState => playerState.GetCards())
                .SelectMany(cards => cards)
                .FirstOrDefault(card => card.DuelistId == duelistId &&
                                        card.YugiohCard.Id == cardId &&
                                        card.CopyNumber == copyNumber);
        }

        public PlayerState GetPlayerWithCard(PlayCard card)
        {
            return GetPlayerStates()
                .FirstOrDefault(playerState => playerState.GetCards().Contains(card));
        }
    }
}