using AssemblyCSharp.Assets.Code.Core.Models.Interface.ModelEventsHandler.Entities;

namespace AssemblyCSharp.Assets.Code.Core.YGOProDeck.Interface
{
    public interface IApiWebRequest
    {
        public void RequestCardImageFromWeb(ModelEvent modelEvent, string zone, string cardId, bool isMonster);
    }
}
