namespace AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities
{
    public abstract class SmartDuelEvent
    {
        public string CardId { get; private set; }
        public string ZoneName { get; private set; }
        public string CardPosition { get; private set; }

        protected SmartDuelEvent(string cardId, string zoneName, string cardPosition)
        {
            CardId = cardId;
            ZoneName = zoneName;
            CardPosition = cardPosition;
        }
    }

    public class SummonCardEvent : SmartDuelEvent
    {
        public SummonCardEvent(string cardId, string zoneName, string cardPosition) : base(cardId, zoneName, cardPosition)
        {
        }
    }

    public class RemoveCardEvent : SmartDuelEvent
    {
        public RemoveCardEvent(string zoneName) : base(null, zoneName, null)
        {
        }
    }

    public class SpellTrapSetEvent : SmartDuelEvent
    {
        public SpellTrapSetEvent(string cardId, string zoneName, bool inDefMode, bool isSet) : base(cardId, zoneName, inDefMode, isSet)
        {
        }
    }
}