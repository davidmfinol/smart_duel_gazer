using System.Collections.Generic;
using System.Linq;
using Code.Core.SmartDuelServer.Entities.EventData.CardEvents;
using Code.Features.SpeedDuel.Models.Zones;

namespace Code.Features.SpeedDuel.Models
{
    public class PlayerState
    {
        public string DuelistId { get; }
        public bool IsOpponent { get; }
        public string PlayMatZonesPath { get; }
        private MultiCardZone HandZone { get; set; }
        private SingleCardZone FieldZone { get; set; }
        private SingleCardZone MainMonsterZone1 { get; set; }
        private SingleCardZone MainMonsterZone2 { get; set; }
        private SingleCardZone MainMonsterZone3 { get; set; }
        private MultiCardZone GraveyardZone { get; set; }
        private MultiCardZone BanishedZone { get; set; }
        private MultiCardZone ExtraDeckZone { get; set; }
        private SingleCardZone SpellTrapZone1 { get; set; }
        private SingleCardZone SpellTrapZone2 { get; set; }
        private SingleCardZone SpellTrapZone3 { get; set; }
        public MultiCardZone DeckZone { get; private set; }

        public PlayerState(string duelistId, bool isOpponent, string playMatZonesPath)
        {
            DuelistId = duelistId;
            IsOpponent = isOpponent;
            PlayMatZonesPath = playMatZonesPath;
            HandZone = new MultiCardZone(ZoneType.Hand);
            FieldZone = new SingleCardZone(ZoneType.Field);
            MainMonsterZone1 = new SingleCardZone(ZoneType.MainMonster1);
            MainMonsterZone2 = new SingleCardZone(ZoneType.MainMonster2);
            MainMonsterZone3 = new SingleCardZone(ZoneType.MainMonster3);
            GraveyardZone = new MultiCardZone(ZoneType.Graveyard);
            BanishedZone = new MultiCardZone(ZoneType.Banished);
            ExtraDeckZone = new MultiCardZone(ZoneType.ExtraDeck);
            SpellTrapZone1 = new SingleCardZone(ZoneType.SpellTrap1);
            SpellTrapZone2 = new SingleCardZone(ZoneType.SpellTrap2);
            SpellTrapZone3 = new SingleCardZone(ZoneType.SpellTrap3);
            DeckZone = new MultiCardZone(ZoneType.Deck);
        }

        public PlayerState CopyWith(
            MultiCardZone handZone = null,
            SingleCardZone fieldZone = null,
            SingleCardZone mainMonsterZone1 = null,
            SingleCardZone mainMonsterZone2 = null,
            SingleCardZone mainMonsterZone3 = null,
            MultiCardZone graveyardZone = null,
            MultiCardZone banishedZone = null,
            MultiCardZone extraDeckZone = null,
            SingleCardZone spellTrapZone1 = null,
            SingleCardZone spellTrapZone2 = null,
            SingleCardZone spellTrapZone3 = null,
            MultiCardZone deckZone = null)
        {
            return new PlayerState(DuelistId, IsOpponent, PlayMatZonesPath)
            {
                HandZone = handZone ?? HandZone,
                FieldZone = fieldZone ?? FieldZone,
                MainMonsterZone1 = mainMonsterZone1 ?? MainMonsterZone1,
                MainMonsterZone2 = mainMonsterZone2 ?? MainMonsterZone2,
                MainMonsterZone3 = mainMonsterZone3 ?? MainMonsterZone3,
                GraveyardZone = graveyardZone ?? GraveyardZone,
                BanishedZone = banishedZone ?? BanishedZone,
                ExtraDeckZone = extraDeckZone ?? ExtraDeckZone,
                SpellTrapZone1 = spellTrapZone1 ?? SpellTrapZone1,
                SpellTrapZone2 = spellTrapZone2 ?? SpellTrapZone2,
                SpellTrapZone3 = spellTrapZone3 ?? SpellTrapZone3,
                DeckZone = deckZone ?? DeckZone
            };
        }

        public Zone GetZone(ZoneType zoneType)
        {
            return GetZones().FirstOrDefault(zone => zone.ZoneType == zoneType);
        }

        public IEnumerable<Zone> GetZones()
        {
            return new List<Zone>
            {
                HandZone,
                FieldZone,
                MainMonsterZone1,
                MainMonsterZone2,
                MainMonsterZone3,
                GraveyardZone,
                BanishedZone,
                ExtraDeckZone,
                SpellTrapZone1,
                SpellTrapZone2,
                SpellTrapZone3,
                DeckZone
            };
        }

        public IEnumerable<PlayCard> GetCards()
        {
            return HandZone.GetCards()
                .Concat(FieldZone.GetCards())
                .Concat(MainMonsterZone1.GetCards())
                .Concat(MainMonsterZone2.GetCards())
                .Concat(MainMonsterZone3.GetCards())
                .Concat(GraveyardZone.GetCards())
                .Concat(BanishedZone.GetCards())
                .Concat(ExtraDeckZone.GetCards())
                .Concat(SpellTrapZone1.GetCards())
                .Concat(SpellTrapZone2.GetCards())
                .Concat(SpellTrapZone3.GetCards())
                .Concat(DeckZone.GetCards());
        }
    }
}