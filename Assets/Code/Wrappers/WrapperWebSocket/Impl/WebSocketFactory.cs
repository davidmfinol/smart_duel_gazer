using Code.Wrappers.WrapperWebSocket.Interface;
using Zenject;

namespace Code.Wrappers.WrapperWebSocket.Impl
{
    public class WebSocketFactory : IWebSocketFactory
    {
        private readonly DiContainer _container;

        public WebSocketFactory(DiContainer container)
        {
            _container = container;
        }

        public IWebSocketProvider CreateWebSocketProvider()
        {
            return _container.Resolve<IWebSocketProvider>();
        }
    }
}
