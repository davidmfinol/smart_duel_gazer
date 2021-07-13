using System.Collections.Generic;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities;

namespace AssemblyCSharp.Assets.Code.Wrappers.WrapperWebSocket.Interface
{
    public interface IWebSocketProvider
    {
        void Init(ISmartDuelEventReceiver receiver);
        void EmitEvent(string eventName, IDictionary<string, object> data);
        void Dispose();
    }
}
