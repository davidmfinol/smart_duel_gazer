using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Code.Core.SmartDuelServer.Entities.EventData.RoomEvents
{
    public class RoomEventData : SmartDuelEventData, IEquatable<RoomEventData>
    {
        [JsonProperty("roomName")] public string RoomName { get; set; }
        [JsonProperty("error")] [CanBeNull] public string Error { get; set; }
        [JsonProperty("duelists")] [CanBeNull] public IList<string> DuelistsIds { get; set; }
        [JsonProperty("duelRoom")] [CanBeNull] public DuelRoom DuelRoom { get; set; }
        [JsonProperty("winnerId")] [CanBeNull] public string WinnerId { get; set; }

        public bool Equals(RoomEventData other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return RoomName == other.RoomName && Error == other.Error && Equals(DuelistsIds, other.DuelistsIds) &&
                   Equals(DuelRoom, other.DuelRoom) && WinnerId == other.WinnerId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((RoomEventData) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (RoomName != null ? RoomName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Error != null ? Error.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DuelistsIds != null ? DuelistsIds.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DuelRoom != null ? DuelRoom.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (WinnerId != null ? WinnerId.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}