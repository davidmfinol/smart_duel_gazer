namespace Code.Core.SmartDuelServer.Entities.EventData.CardEvents
{
    public enum CardPosition
    {
        FaceUp = 1,
        FaceDown = 2,
        FaceUpDefence = 3,
        FaceDownDefence = 4
    }

    public static class CardPositionExtensions
    {
        public static bool IsDefence(this CardPosition value)
        {
            return value == CardPosition.FaceUpDefence || value == CardPosition.FaceDownDefence;
        }
    }
}