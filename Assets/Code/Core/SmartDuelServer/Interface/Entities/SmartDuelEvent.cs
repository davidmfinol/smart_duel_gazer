namespace AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities
{
    public abstract class SmartDuelEvent
    {
        public string CardId { get; private set; }
        public string ZoneName { get; private set; }

        protected SmartDuelEvent(string cardId, string zoneName)
        {
            CardId = cardId;
            ZoneName = zoneName;
        }
    }

    public class SummonEvent : SmartDuelEvent
    {
        public SummonEvent(string cardId, string zoneName) : base(cardId, zoneName)
        {
        }
    }

    public class RemoveCardEvent : SmartDuelEvent
    {
        public RemoveCardEvent(string zoneName) : base(null, zoneName)
        {
        }
    }
}