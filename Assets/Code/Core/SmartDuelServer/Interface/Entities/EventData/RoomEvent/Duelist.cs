using System.Collections.Generic;
using Newtonsoft.Json;

namespace Code.Core.SmartDuelServer.Interface.Entities.EventData.RoomEvent
{
    public class Duelist
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("deckList")] public IList<int> DeckList { get; set; }

        public Duelist(
            string id,
            IList<int> deckList)
        {
            Id = id;
            DeckList = deckList;
        }
    }
}