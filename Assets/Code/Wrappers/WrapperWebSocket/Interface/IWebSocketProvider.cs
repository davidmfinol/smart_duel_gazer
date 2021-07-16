using Code.Core.SmartDuelServer.Interface.Entities;

namespace Code.Wrappers.WrapperWebSocket.Interface
{
    public interface IWebSocketProvider
    {
        void Init(ISmartDuelEventReceiver receiver);
        void EmitEvent(string eventName, string json);
        void Dispose();
    }
}
