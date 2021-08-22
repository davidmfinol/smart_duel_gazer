using Code.Core.Config.Entities;
using Code.Core.DataManager.Connections.Entities;
using Code.Core.Storage.Connections;
using Code.Core.Storage.Connections.Models;

namespace Code.Core.DataManager.Connections
{
    public interface IConnectionDataManager
    {
        ConnectionInfo GetConnectionInfo(bool forceLocalInfo = false);
        void SaveConnectionInfo(ConnectionInfo connectionInfo);
        bool UseOnlineDuelRoom();
        void SaveUseOnlineDuelRoom(bool value);
    }

    public class ConnectionDataManager : IConnectionDataManager
    {
        private readonly IAppConfig _appConfig;
        private readonly IConnectionStorageProvider _connectionStorageProvider;

        public ConnectionDataManager(
            IAppConfig appConfig,
            IConnectionStorageProvider connectionStorageProvider)
        {
            _appConfig = appConfig;
            _connectionStorageProvider = connectionStorageProvider;
        }

        public ConnectionInfo GetConnectionInfo(bool forceLocalInfo = false)
        {
            return UseOnlineDuelRoom() && !forceLocalInfo ? GetOnlineConnectionInfo() : GetLocalConnectionInfo();
        }

        private ConnectionInfo GetOnlineConnectionInfo()
        {
            return new ConnectionInfo(_appConfig.SmartDuelServerAddress, _appConfig.SmartDuelServerPort);
        }

        private ConnectionInfo GetLocalConnectionInfo()
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

        public bool UseOnlineDuelRoom()
        {
            return _connectionStorageProvider.UseOnlineDuelRoom();
        }

        public void SaveUseOnlineDuelRoom(bool value)
        {
            _connectionStorageProvider.SaveUseOnlineDuelRoom(value);
        }
    }
}