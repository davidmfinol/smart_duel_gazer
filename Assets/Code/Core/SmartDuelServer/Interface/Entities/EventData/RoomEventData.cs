using AssemblyCSharp.Assets.Code.Core.General.Extensions;
using Newtonsoft.Json.Linq;

namespace AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities.EventData
{
    public class RoomEventData : SmartDuelEventData
    {
        public string RoomName { get; private set; }

        public RoomEventData(string roomName)
        {
            RoomName = roomName;
        }

        public static RoomEventData FromJson(JToken data)
        {
            var roomName = data["roomName"].ToString().RemoveQuotes();

            return new RoomEventData(roomName);
        }
    }
}