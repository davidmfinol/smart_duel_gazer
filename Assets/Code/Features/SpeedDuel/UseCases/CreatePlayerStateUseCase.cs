using System.Collections.Generic;
using System.Linq;
using Code.Core.SmartDuelServer.Interface.Entities.EventData.CardEvents;
using Code.Core.SmartDuelServer.Interface.Entities.EventData.RoomEvents;
using Code.Features.SpeedDuel.Models;

namespace Code.Features.SpeedDuel.UseCases
{
    public interface ICreatePlayerStateUseCase
    {
        PlayerState Execute(Duelist duelist, bool isOpponent);
    }
    
    public class CreatePlayerStateUseCase : ICreatePlayerStateUseCase
    {
        private const string UserPlayMatZonesPath = "UserPlayMat/Zones";
        private const string OpponentPlayMatZonesPath = "OpponentPlayMat/Zones";
        
        private readonly ICreatePlayCardUseCase _createPlayCardUseCase;

        public CreatePlayerStateUseCase(
            ICreatePlayCardUseCase createPlayCardUseCase)
        {
            _createPlayCardUseCase = createPlayCardUseCase;
        }
        
        public PlayerState Execute(Duelist duelist, bool isOpponent)
        {
            var playCards = new List<PlayCard>();
            foreach (var cardId in duelist.DeckList)
            {
                var copyNumber = playCards.Count(card => card.Id == cardId) + 1;
                // TODO: when card type is available, move the correct cards to the extra deck
                const ZoneType zoneType = ZoneType.Deck;

                var playCard = _createPlayCardUseCase.Execute(cardId, copyNumber, zoneType);
                
                playCards.Add(playCard);
            }

            var playMatZonesPath = isOpponent ? OpponentPlayMatZonesPath : UserPlayMatZonesPath;

            // TODO: when card type is available, move the correct cards to the extra deck
            var playerState = new PlayerState(duelist.Id, isOpponent, playMatZonesPath);
            playerState = playerState.CopyWith(
                deckZone: playerState.DeckZone.CopyWith(playCards)
            );

            return playerState;
        }
    }
}