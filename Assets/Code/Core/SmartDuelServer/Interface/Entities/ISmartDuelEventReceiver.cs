using Newtonsoft.Json.Linq;

namespace Code.Core.SmartDuelServer.Interface.Entities
{
    public interface ISmartDuelEventReceiver
    {
        void OnEventReceived(string scope, string action, JToken json);
    }
}
