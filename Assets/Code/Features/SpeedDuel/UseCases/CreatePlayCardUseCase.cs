using Code.Core.SmartDuelServer.Entities.EventData.CardEvents;
using Code.Features.SpeedDuel.Models;

namespace Code.Features.SpeedDuel.UseCases
{
    public interface ICreatePlayCardUseCase
    {
        PlayCard Execute(string duelistId, int cardId, int copyNumber, ZoneType zoneType = ZoneType.Deck,
            CardPosition position = CardPosition.FaceUp);
    }

    public class CreatePlayCardUseCase : ICreatePlayCardUseCase
    {
        public PlayCard Execute(string duelistId, int cardId, int copyNumber, ZoneType zoneType = ZoneType.Deck,
            CardPosition position = CardPosition.FaceUp)
        {
            var yugiohCard = new YugiohCard(cardId);
            return new PlayCard(duelistId, yugiohCard, copyNumber, zoneType, position);
        }
    }
}