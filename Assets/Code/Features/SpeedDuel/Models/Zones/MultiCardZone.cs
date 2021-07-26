using System.Collections.Generic;
using Code.Core.SmartDuelServer.Interface.Entities.EventData.CardEvents;

namespace Code.Features.SpeedDuel.Models.Zones
{
    public class MultiCardZone : Zone
    {
        public IList<PlayCard> Cards { get; private set; }

        public MultiCardZone(ZoneType zoneType) : base(zoneType)
        {
            Cards = new List<PlayCard>();
        }

        public MultiCardZone CopyWith(IList<PlayCard> cards)
        {
            return new MultiCardZone(ZoneType)
            {
                Cards = cards
            };
        }

        public override IEnumerable<PlayCard> GetCards()
        {
            return Cards;
        }
    }
}