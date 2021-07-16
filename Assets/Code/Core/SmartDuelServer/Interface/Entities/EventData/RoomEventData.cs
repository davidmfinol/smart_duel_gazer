using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Code.Core.SmartDuelServer.Interface.Entities.EventData
{
    public class RoomEventData : SmartDuelEventData
    {
        public string RoomName { get; }
        public string Error { get; }
        public IList<string> Duelists { get; }

        public RoomEventData(string roomName, string error = null, IList<string> duelists = null)
        {
            RoomName = roomName;
            Error = error;
            Duelists = duelists;
        }

        public static RoomEventData FromJson(JToken data)
        {
            var roomName = data["roomName"]?.ToObject<string>();
            var error = data["error"]?.ToObject<string>();
            var duelists = data["duelists"]?.ToObject<IList<string>>();

            return new RoomEventData(roomName, error, duelists);
        }
    }
}