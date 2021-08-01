using System.Collections.Generic;
using Newtonsoft.Json;

namespace Code.Core.SmartDuelServer.Entities.EventData.RoomEvents
{
    public class Duelist
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("deckList")] public IList<int> DeckList { get; set; }
    }
}