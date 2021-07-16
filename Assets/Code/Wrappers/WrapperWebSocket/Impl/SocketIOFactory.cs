using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using Dpoch.SocketIO;
using Zenject;

namespace Code.Wrappers.WrapperWebSocket.Impl
{
    public class SocketIOFactory : IFactory<SocketIO>
    {
        private const string ConnectionUrl = "ws://{0}:{1}/socket.io/?EIO=3&transport=websocket";

        private readonly IDataManager _dataManager;

        public SocketIOFactory(IDataManager dataManager)
        {
            _dataManager = dataManager;
        }

        public SocketIO Create()
        {
            var connectionInfo = _dataManager.GetConnectionInfo();
            var url = string.Format(ConnectionUrl, connectionInfo?.IpAddress, connectionInfo?.Port);

            return new SocketIO(url);
        }
    }
}
