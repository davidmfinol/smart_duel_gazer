using System.Collections.Generic;
using Code.Core.SmartDuelServer.Interface.Entities.EventData.RoomEvent;
using Newtonsoft.Json;

namespace Code.Core.SmartDuelServer.Interface.Entities.EventData
{
    public class RoomEventData : SmartDuelEventData
    {
        [JsonProperty("roomName")] public string RoomName { get; set; }
        [JsonProperty("error")] public string Error { get; set; }
        [JsonProperty("duelists")] public IList<string> DuelistsIds { get; set; }
        [JsonProperty("duelRoom")] public DuelRoom DuelRoom { get; set; }

        public RoomEventData(
            string roomName,
            string error = null,
            IList<string> duelistsIds = null)
        {
            RoomName = roomName;
            Error = error;
            DuelistsIds = duelistsIds;
        }
    }
}