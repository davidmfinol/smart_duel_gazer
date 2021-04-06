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

    public class PlayCardEvent : SmartDuelEvent
    {
        public PlayCardEvent(string cardId, string zoneName, string cardPosition) : base(cardId, zoneName, cardPosition)
        {
        }
    }

    public class RemoveCardEvent : SmartDuelEvent
    {
        public RemoveCardEvent(string zoneName) : base(null, zoneName, null)
        {
        }
    }
}