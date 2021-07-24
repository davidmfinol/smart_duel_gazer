using System.Collections.Generic;
using Newtonsoft.Json;

namespace Code.Core.SmartDuelServer.Interface.Entities.EventData.RoomEvents
{
    public class DuelRoom
    {
        [JsonProperty("roomName")] public string RoomName { get; set; }
        [JsonProperty("duelists")] public IList<Duelist> Duelists { get; set; }
        [JsonProperty("duelistLimit")] public int DuelistLimit { get; set; }
        public string DuelistToSpectate { get; set; }
    }
}