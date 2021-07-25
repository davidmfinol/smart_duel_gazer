using System.Collections.Generic;
using Code.Core.SmartDuelServer.Interface.Entities.EventData.CardEvents;

namespace Code.Features.SpeedDuel.Models.Zones
{
    public abstract class Zone
    {
        public ZoneType ZoneType { get; }

        protected Zone(ZoneType zoneType)
        {
            ZoneType = zoneType;
        }

        public abstract IEnumerable<PlayCard> GetCards();
    }
}