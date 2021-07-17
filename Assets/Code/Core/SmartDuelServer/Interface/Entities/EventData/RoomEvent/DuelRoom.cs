using System.Collections.Generic;
using Newtonsoft.Json;

namespace Code.Core.SmartDuelServer.Interface.Entities.EventData.RoomEvent
{
    public class DuelRoom
    {
        [JsonProperty("roomName")] public string RoomName { get; set; }
        [JsonProperty("duelists")] public IList<Duelist> Duelists { get; set; }
        [JsonProperty("duelistLimit")] public int DuelistLimit { get; set; }
        public string DuelistToSpectate { get; set; }

        public DuelRoom(
            string roomName,
            IList<Duelist> duelists,
            int duelistLimit)
        {
            RoomName = roomName;
            Duelists = duelists;
            DuelistLimit = duelistLimit;
        }
    }
}