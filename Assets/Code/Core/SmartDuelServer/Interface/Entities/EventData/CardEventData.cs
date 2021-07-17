using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Code.Core.SmartDuelServer.Interface.Entities.EventData
{
    public class CardEventData : SmartDuelEventData
    {
        [JsonProperty("duelistId")] public string DuelistId { get; set; }
        [JsonProperty("cardId")] public string CardId { get; set; }
        [JsonProperty("copyNumber")] public int CopyNumber { get; set; }
        [JsonProperty("zoneName")] public string ZoneName { get; set; }

        [JsonProperty("cardPosition")]
        [JsonConverter(typeof(StringEnumConverter))]
        public CardPosition CardPosition { get; set; }

        public CardEventData(
            string duelistId,
            string cardId,
            int copyNumber,
            string zoneName = null,
            CardPosition cardPosition = CardPosition.None)
        {
            DuelistId = duelistId;
            CardId = cardId;
            CopyNumber = copyNumber;
            ZoneName = zoneName;
            CardPosition = cardPosition;
        }
    }

    public enum CardPosition
    {
        FaceUp,
        FaceDown,
        FaceUpDefence,
        FaceDownDefence,
        None
    }
}