using Code.Wrappers.WrapperWebSocket.Interface;
using Zenject;

namespace Code.Wrappers.WrapperWebSocket.Impl
{
    public class WebSocketFactory : IWebSocketFactory
    {
        private readonly DiContainer _di;

        public WebSocketFactory(DiContainer di)
        {
            _di = di;
        }

        public IWebSocketProvider CreateWebSocketProvider()
        {
            return _di.Resolve<IWebSocketProvider>();
        }
    }
}
