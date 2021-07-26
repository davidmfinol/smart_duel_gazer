using Code.Core.SmartDuelServer.Interface.Entities.EventData.CardEvents;

namespace Code.Features.SpeedDuel.Models
{
    public class PlayCard
    {
        public int Id { get; }
        public int CopyNumber { get; }
        public ZoneType ZoneType { get; }
        public CardPosition CardPosition { get; }

        public PlayCard(int id, int copyNumber, ZoneType zoneType, CardPosition cardPosition)
        {
            Id = id;
            CopyNumber = copyNumber;
            ZoneType = zoneType;
            CardPosition = cardPosition;
        }

        public PlayCard CopyWith(ZoneType zoneType = default, CardPosition cardPosition = default)
        {
            return new PlayCard(
                Id,
                CopyNumber,
                zoneType != default ? zoneType : ZoneType,
                cardPosition != default ? cardPosition : CardPosition) ;
        }
    }
}