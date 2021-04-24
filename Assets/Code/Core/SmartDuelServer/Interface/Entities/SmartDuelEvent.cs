using System;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;
using Newtonsoft.Json.Linq;

namespace AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities
{
    public abstract class SmartDuelEvent
    {
        public string CardId { get; private set; }
        public string ZoneName { get; private set; }
        public CardPosition CardPosition { get; private set; }

        protected SmartDuelEvent(string cardId, string zoneName, CardPosition cardPosition)
        {
            CardId = cardId;
            ZoneName = zoneName;
            CardPosition = cardPosition;
        }
    }

    public class PlayCardEvent : SmartDuelEvent
    {
        public PlayCardEvent(string cardId, string zoneName, CardPosition cardPosition) : base(cardId, zoneName, cardPosition)
        {
        }

        public static PlayCardEvent FromJson(JToken data)
        {
            var cardId = data["cardId"].ToString().RemoveQuotes();
            var zoneName = data["zoneName"].ToString().RemoveQuotes();
            var cardPositionString = data["cardPosition"].ToString().RemoveQuotes();
            var parseSuccess = Enum.TryParse<CardPosition>(cardPositionString, ignoreCase: true, out var cardPosition);

            return new PlayCardEvent(cardId, zoneName, parseSuccess ? cardPosition : CardPosition.None);
        }
    }

    public class RemoveCardEvent : SmartDuelEvent
    {
        public RemoveCardEvent(string zoneName) : base(default, zoneName, CardPosition.None)
        {
        }

        public static RemoveCardEvent FromJson(JToken data)
        {
            var zoneName = data["zoneName"].ToString().RemoveQuotes();

            return new RemoveCardEvent(zoneName);
        }
    }
}