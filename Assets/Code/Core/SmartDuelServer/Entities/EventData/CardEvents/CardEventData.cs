using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Code.Core.SmartDuelServer.Entities.EventData.CardEvents
{
    public class CardEventData : SmartDuelEventData, IEquatable<CardEventData>
    {
        [JsonProperty("duelistId")] public string DuelistId { get; set; }
        [JsonProperty("cardId")] public int CardId { get; set; }
        [JsonProperty("copyNumber")] public int CopyNumber { get; set; }

        [JsonProperty("zoneName")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ZoneType? ZoneType { get; set; }

        [JsonProperty("cardPosition")]
        [JsonConverter(typeof(StringEnumConverter))]
        public CardPosition? CardPosition { get; set; }

        public bool Equals(CardEventData other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return DuelistId == other.DuelistId && CardId == other.CardId && CopyNumber == other.CopyNumber &&
                   ZoneType == other.ZoneType && CardPosition == other.CardPosition;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((CardEventData) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (DuelistId != null ? DuelistId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ CardId;
                hashCode = (hashCode * 397) ^ CopyNumber;
                hashCode = (hashCode * 397) ^ ZoneType.GetHashCode();
                hashCode = (hashCode * 397) ^ CardPosition.GetHashCode();
                return hashCode;
            }
        }
    }
}