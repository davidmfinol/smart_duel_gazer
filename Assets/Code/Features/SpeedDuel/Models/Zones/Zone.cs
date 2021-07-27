using Code.Core.SmartDuelServer.Entities.EventData.CardEvents;

namespace Code.Features.SpeedDuel.Models.Zones
{
    public abstract class Zone
    {
        public ZoneType ZoneType { get; }

        protected Zone(ZoneType zoneType)
        {
            ZoneType = zoneType;
        }
    }
}