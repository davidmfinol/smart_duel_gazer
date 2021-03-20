namespace AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities
{
    public abstract class SmartDuelEvent
    {
        public string CardId { get; private set; }
        public string ZoneName { get; private set; }
        public bool InDefMode { get; private set; }
        public bool IsSet { get; private set; }

        protected SmartDuelEvent(string cardId, string zoneName, bool inDefMode, bool isSet)
        {
            CardId = cardId;
            ZoneName = zoneName;
            InDefMode = inDefMode;
            IsSet = isSet;
        }
    }

    public class SummonEvent : SmartDuelEvent
    {
        public SummonEvent(string cardId, string zoneName, bool inDefMode, bool isSet) : base(cardId, zoneName, inDefMode, isSet)
        {
        }
    }

    public class RemoveCardEvent : SmartDuelEvent
    {
        public RemoveCardEvent(string zoneName, bool inDefMode, bool isSet) : base(null, zoneName, inDefMode, isSet)
        {
        }
    }
    public class PositionChangeEvent : SmartDuelEvent
    {
        public PositionChangeEvent(string zoneName, bool inDefMode, bool isSet) : base(null, zoneName, inDefMode, isSet)
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