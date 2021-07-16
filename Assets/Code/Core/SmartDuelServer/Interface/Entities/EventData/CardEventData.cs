using System;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;
using Newtonsoft.Json.Linq;

namespace Code.Core.SmartDuelServer.Interface.Entities.EventData
{
    public class CardEventData : SmartDuelEventData
    {
        public string CardId { get; }
        public int CopyNumber { get; }
        public string ZoneName { get; }
        public CardPosition CardPosition { get; }

        public CardEventData(string cardId, int copyNumber, string zoneName, CardPosition cardPosition)
        {
            CardId = cardId;
            CopyNumber = copyNumber;
            ZoneName = zoneName;
            CardPosition = cardPosition;
        }

        public static CardEventData FromJson(JToken data)
        {
            var cardId = data["cardId"].ToString().RemoveQuotes();

            var copyNumberString = data["copyNumber"].ToString().RemoveQuotes();
            var hasCopyNumber = int.TryParse(copyNumberString, out var copyNumber);

            var zoneName = data["zoneName"].ToString().RemoveQuotes();

            var cardPositionString = data["cardPosition"].ToString().RemoveQuotes();
            var hasCardPosition = Enum.TryParse<CardPosition>(cardPositionString, ignoreCase: true, out var cardPosition);

            return new CardEventData(
                cardId,
                hasCopyNumber ? copyNumber : 0,
                zoneName,
                hasCardPosition ? cardPosition : CardPosition.None);
        }
    }

    public enum CardPosition
    {
        FaceUp,
        FaceDown,
        FaceUpDefence,
        FaceDownDefence,
        None,
    }
}