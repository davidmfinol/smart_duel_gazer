using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Code.Core.SmartDuelServer.Entities.EventData.RoomEvents
{
    public class RoomEventData : SmartDuelEventData
    {
        [JsonProperty("roomName")] public string RoomName { get; set; }
        [JsonProperty("error")] [CanBeNull] public string Error { get; set; }
        [JsonProperty("duelists")] [CanBeNull] public IList<string> DuelistsIds { get; set; }
        [JsonProperty("duelRoom")] [CanBeNull] public DuelRoom DuelRoom { get; set; }
        [JsonProperty("winnerId")] [CanBeNull] public string WinnerId { get; set; }
    }
}