using System.Collections.Generic;
using Code.Core.SmartDuelServer.Entities.EventData.CardEvents;
using Newtonsoft.Json;

namespace Code.Features.SpeedDuel.Models.Zones
{
    public class MultiCardZone : Zone
    {
        [JsonProperty("cards")] public IList<PlayCard> Cards { get; set; }

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

        public virtual IEnumerable<PlayCard> GetCards()
        {
            return Cards;
        }
    }
}