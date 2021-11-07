using System.Collections.Generic;
using System.Linq;
using Code.Core.SmartDuelServer.Entities.EventData.CardEvents;
using Code.Features.SpeedDuel.Models.Zones;
using Zenject.Internal;

namespace Code.Features.SpeedDuel.Models
{
    public class PlayerState
    {
        public string DuelistId { get; }
        public bool IsOpponent { get; }
        public string PlayMatZonesPath { get; }
        private MultiCardZone HandZone { get; }
        private SingleCardZone FieldZone { get; }
        private SingleCardZone MainMonsterZone1 { get; }
        private SingleCardZone MainMonsterZone2 { get; }
        private SingleCardZone MainMonsterZone3 { get; }
        private MultiCardZone GraveyardZone { get; }
        private MultiCardZone BanishedZone { get; }
        private MultiCardZone ExtraDeckZone { get; }
        private SingleCardZone SpellTrapZone1 { get; }
        private SingleCardZone SpellTrapZone2 { get; }
        private SingleCardZone SpellTrapZone3 { get; }
        private MultiCardZone DeckZone { get; }

        public PlayerState(
            string duelistId,
            bool isOpponent,
            string playMatZonesPath,
            MultiCardZone handZone,
            SingleCardZone fieldZone,
            SingleCardZone mainMonsterZone1,
            SingleCardZone mainMonsterZone2,
            SingleCardZone mainMonsterZone3,
            MultiCardZone graveyardZone,
            MultiCardZone banishedZone,
            MultiCardZone extraDeckZone,
            SingleCardZone spellTrapZone1,
            SingleCardZone spellTrapZone2,
            SingleCardZone spellTrapZone3,
            MultiCardZone deckZone
        )
        {
            DuelistId = duelistId;
            IsOpponent = isOpponent;
            PlayMatZonesPath = playMatZonesPath;
            HandZone = handZone;
            FieldZone = fieldZone;
            MainMonsterZone1 = mainMonsterZone1;
            MainMonsterZone2 = mainMonsterZone2;
            MainMonsterZone3 = mainMonsterZone3;
            GraveyardZone = graveyardZone;
            BanishedZone = banishedZone;
            ExtraDeckZone = extraDeckZone;
            SpellTrapZone1 = spellTrapZone1;
            SpellTrapZone2 = spellTrapZone2;
            SpellTrapZone3 = spellTrapZone3;
            DeckZone = deckZone;
        }

        public PlayerState CopyWith(IList<Zone> newZones)
        {
            return new PlayerState(
                DuelistId,
                IsOpponent,
                PlayMatZonesPath,
                newZones.First(z => z.ZoneType == ZoneType.Hand) as MultiCardZone,
                newZones.First(z => z.ZoneType == ZoneType.Field) as SingleCardZone,
                newZones.First(z => z.ZoneType == ZoneType.MainMonster1) as SingleCardZone,
                newZones.First(z => z.ZoneType == ZoneType.MainMonster2) as SingleCardZone,
                newZones.First(z => z.ZoneType == ZoneType.MainMonster3) as SingleCardZone,
                newZones.First(z => z.ZoneType == ZoneType.Graveyard) as MultiCardZone,
                newZones.First(z => z.ZoneType == ZoneType.Banished) as MultiCardZone,
                newZones.First(z => z.ZoneType == ZoneType.ExtraDeck) as MultiCardZone,
                newZones.First(z => z.ZoneType == ZoneType.SpellTrap1) as SingleCardZone,
                newZones.First(z => z.ZoneType == ZoneType.SpellTrap2) as SingleCardZone,
                newZones.First(z => z.ZoneType == ZoneType.SpellTrap3) as SingleCardZone,
                newZones.First(z => z.ZoneType == ZoneType.Deck) as MultiCardZone
            );
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