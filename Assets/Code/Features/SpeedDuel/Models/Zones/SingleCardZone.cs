using Code.Core.SmartDuelServer.Interface.Entities.EventData.CardEvents;
using UnityEngine;

namespace Code.Features.SpeedDuel.Models.Zones
{
    public class SingleCardZone : Zone
    {
        public PlayCard Card { get; private set; }
        public GameObject SetCardModel { get; private set; }
        public GameObject MonsterModel { get; private set; }
        
        public SingleCardZone(ZoneType zoneType) : base(zoneType)
        {
        }

        public SingleCardZone CopyWith(PlayCard card = null, GameObject setCardModel = null, GameObject monsterModel = null)
        {
            return new SingleCardZone(ZoneType)
            {
                Card = card ?? Card,
                SetCardModel = setCardModel ? setCardModel : SetCardModel,
                MonsterModel = monsterModel ? monsterModel : MonsterModel
            };
        }
    }
}