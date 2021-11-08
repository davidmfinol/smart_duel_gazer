using Code.Core.SmartDuelServer.Entities.EventData.CardEvents;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Code.Features.SpeedDuel.Models
{
    public class PlayCard
    {
        [JsonProperty("duelistId")] public string DuelistId { get; set; }
        [JsonProperty("yugiohCard")] public YugiohCard YugiohCard { get; set; }
        [JsonProperty("copyNumber")] public int CopyNumber { get; set; }

        [JsonProperty("cardPosition")]
        [JsonConverter(typeof(StringEnumConverter))]
        public CardPosition CardPosition { get; set; }

        [JsonProperty("zoneType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ZoneType ZoneType { get; set; }

        public PlayCard(YugiohCard yugiohCard, int copyNumber, ZoneType zoneType, CardPosition cardPosition)
        {
            YugiohCard = yugiohCard;
            CopyNumber = copyNumber;
            ZoneType = zoneType;
            CardPosition = cardPosition;
        }

        public PlayCard CopyWith(ZoneType zoneType = default, CardPosition cardPosition = default)
        {
            return new PlayCard(
                YugiohCard,
                CopyNumber,
                zoneType != default ? zoneType : ZoneType,
                cardPosition != default ? cardPosition : CardPosition);
        }
    }

    public class YugiohCard
    {
        [JsonProperty("id")] public int Id { get; set; }

        public YugiohCard(int id)
        {
            Id = id;
        }
    }
}