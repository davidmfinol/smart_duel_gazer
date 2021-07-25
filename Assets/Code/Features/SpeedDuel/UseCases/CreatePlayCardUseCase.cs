using Code.Core.SmartDuelServer.Interface.Entities.EventData.CardEvents;
using Code.Features.SpeedDuel.Models;

namespace Code.Features.SpeedDuel.UseCases
{
    public interface ICreatePlayCardUseCase
    {
        PlayCard Execute(int cardId, int copyNumber, ZoneType zoneType = ZoneType.Deck,
            CardPosition position = CardPosition.FaceUp);
    }

    public class CreatePlayCardUseCase : ICreatePlayCardUseCase
    {
        public PlayCard Execute(int cardId, int copyNumber, ZoneType zoneType = ZoneType.Deck,
            CardPosition position = CardPosition.FaceUp)
        {
            return new PlayCard(cardId, copyNumber, zoneType, position);
        }
    }
}