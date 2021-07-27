using Code.Core.DataManager.Connections.Entities;
using Code.Core.Storage.Connection;
using Code.Core.Storage.Connection.Models;
using Zenject;

namespace Code.Core.DataManager.Connections
{
    public interface IConnectionDataManager
    {
        ConnectionInfo GetConnectionInfo();
        void SaveConnectionInfo(ConnectionInfo connectionInfo);
    }

    public class ConnectionDataManager : IConnectionDataManager
    {
        private readonly IConnectionStorageProvider _connectionStorageProvider;

        [Inject]
        public ConnectionDataManager(
            IConnectionStorageProvider connectionStorageProvider)
        {
            _connectionStorageProvider = connectionStorageProvider;
        }

        public ConnectionInfo GetConnectionInfo()
        {
            var model = _connectionStorageProvider.GetConnectionInfo();

            return model == null
                ? null
                : new ConnectionInfo(model.IpAddress, model.Port);
        }

        public void SaveConnectionInfo(ConnectionInfo connectionInfo)
        {
            var model = new ConnectionInfoModel(connectionInfo.IpAddress, connectionInfo.Port);

            _connectionStorageProvider.SaveConnectionInfo(model);
        }
    }
}