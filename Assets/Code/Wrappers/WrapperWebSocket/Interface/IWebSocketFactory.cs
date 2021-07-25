namespace Code.Wrappers.WrapperWebSocket.Interface
{
    public interface IWebSocketFactory
    {
        IWebSocketProvider CreateWebSocketProvider();
    }
}