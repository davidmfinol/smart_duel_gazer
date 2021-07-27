using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Code.Core.SmartDuelServer.Entities.EventData.CardEvents
{
    public class CardEventData : SmartDuelEventData
    {
        [JsonProperty("duelistId")] public string DuelistId { get; set; }
        [JsonProperty("cardId")] public int CardId { get; set; }
        [JsonProperty("copyNumber")] public int CopyNumber { get; set; }

        [JsonProperty("zoneName")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ZoneType ZoneType { get; set; }

        [JsonProperty("cardPosition")]
        [JsonConverter(typeof(StringEnumConverter))]
        public CardPosition CardPosition { get; set; }
    }
}