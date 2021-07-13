using AssemblyCSharp.Assets.Code.Wrappers.WrapperWebSocket.Interface;
using Zenject;

namespace AssemblyCSharp.Assets.Code.Wrappers.WrapperWebSocket.Impl
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
            return _container.Instantiate<IWebSocketProvider>();
        }
    }
}
