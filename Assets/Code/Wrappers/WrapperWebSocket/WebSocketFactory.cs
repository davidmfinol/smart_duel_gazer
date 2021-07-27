using Zenject;

namespace Code.Wrappers.WrapperWebSocket
{
    public interface IWebSocketFactory
    {
        IWebSocketProvider CreateWebSocketProvider();
    }
    
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
