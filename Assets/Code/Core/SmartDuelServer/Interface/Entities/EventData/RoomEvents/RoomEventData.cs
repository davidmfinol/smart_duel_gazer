using System.Collections.Generic;
using Newtonsoft.Json;

namespace Code.Core.SmartDuelServer.Interface.Entities.EventData.RoomEvents
{
    public class RoomEventData : SmartDuelEventData
    {
        [JsonProperty("roomName")] public string RoomName { get; set; }
        [JsonProperty("error")] public string Error { get; set; }
        [JsonProperty("duelists")] public IList<string> DuelistsIds { get; set; }
        [JsonProperty("duelRoom")] public DuelRoom DuelRoom { get; set; }
        [JsonProperty("winnerId")] public string WinnerId { get; set; }

        public RoomEventData(string roomName)
        {
            RoomName = roomName;
        }
    }
}