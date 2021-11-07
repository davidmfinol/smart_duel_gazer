using System.Collections.Generic;
using Code.Features.SpeedDuel.Models.Zones;
using Newtonsoft.Json;

namespace Code.Core.SmartDuelServer.Entities.EventData.RoomEvents
{
    public class Duelist
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("deckList")] public IList<int> DeckList { get; set; }
        [JsonProperty("handZone")] public MultiCardZone HandZone { get; set; }
        [JsonProperty("fieldZone")] public SingleCardZone FieldZone { get; set; }
        [JsonProperty("mainMonsterZone1")] public SingleCardZone MainMonsterZone1 { get; set; }
        [JsonProperty("mainMonsterZone2")] public SingleCardZone MainMonsterZone2 { get; set; }
        [JsonProperty("mainMonsterZone3")] public SingleCardZone MainMonsterZone3 { get; set; }
        [JsonProperty("graveyardZone")] public MultiCardZone GraveyardZone { get; set; }
        [JsonProperty("banishedZone")] public MultiCardZone BanishedZone { get; set; }
        [JsonProperty("extraDeckZone")] public MultiCardZone ExtraDeckZone { get; set; }
        [JsonProperty("spellTrapZone1")] public SingleCardZone SpellTrapZone1 { get; set; }
        [JsonProperty("spellTrapZone2")] public SingleCardZone SpellTrapZone2 { get; set; }
        [JsonProperty("spellTrapZone3")] public SingleCardZone SpellTrapZone3 { get; set; }
        [JsonProperty("deckZone")] public MultiCardZone DeckZone { get; set; }
    }
}