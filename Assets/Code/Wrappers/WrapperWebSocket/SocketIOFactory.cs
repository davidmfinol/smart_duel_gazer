using Code.Core.DataManager;
using Code.Core.Logger;
using Dpoch.SocketIO;
using Zenject;

namespace Code.Wrappers.WrapperWebSocket
{
    public class SocketIOFactory : IFactory<SocketIO>
    {
        private const string Tag = "SocketIOFactory";

        private const string ConnectionUrl = "{0}{1}:{2}/socket.io/?EIO=3&transport=websocket";

        private const string WebSocketPrefix = "ws://";
        private const string WebSocketSecurePrefix = "wss://";
        private const string HttpPrefix = "http://";
        private const string HttpSecurePrefix = "https://";

        private readonly IDataManager _dataManager;
        private readonly IAppLogger _logger;

        public SocketIOFactory(
            IDataManager dataManager,
            IAppLogger logger)
        {
            _dataManager = dataManager;
            _logger = logger;
        }

        public SocketIO Create()
        {
            var connectionInfo = _dataManager.GetConnectionInfo();
            var webSocketPrefix = connectionInfo.IpAddress.StartsWith(HttpSecurePrefix)
                ? WebSocketSecurePrefix
                : WebSocketPrefix;

            var ipAddress = connectionInfo.IpAddress.Replace(HttpPrefix, "").Replace(HttpSecurePrefix, "");
            var uri = string.Format(ConnectionUrl, webSocketPrefix, ipAddress, connectionInfo.Port);

            _logger.Log(Tag, $"Creating a socket with uri: {uri}");

            return new SocketIO(uri);
        }
    }
}