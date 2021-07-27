using System.Collections.Generic;
using Code.Core.SmartDuelServer.Entities.EventData.CardEvents;
using UnityEngine;

namespace Code.Features.SpeedDuel.Models.Zones
{
    public class SingleCardZone : Zone
    {
        private PlayCard Card { get; set; }
        public GameObject SetCardModel { get; private set; }
        public GameObject MonsterModel { get; private set; }

        public SingleCardZone(ZoneType zoneType) : base(zoneType)
        {
        }

        public SingleCardZone CopyWith(PlayCard card = null, GameObject setCardModel = null, GameObject monsterModel = null)
        {
            return new SingleCardZone(ZoneType)
            {
                Card = card,
                SetCardModel = setCardModel,
                MonsterModel = monsterModel
            };
        }

        public virtual IEnumerable<PlayCard> GetCards()
        {
            return Card == null ? new List<PlayCard>() : new List<PlayCard> {Card};
        }
    }
}