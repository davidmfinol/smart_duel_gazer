using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Connection;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Connection.Entities;
using AssemblyCSharp.Assets.Code.Core.Storage.Interface.Connection;
using AssemblyCSharp.Assets.Code.Core.Storage.Interface.Connection.Models;
using Zenject;

namespace AssemblyCSharp.Assets.Code.Core.DataManager.Impl.Connection
{
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
            if (model == null)
            {
                return null;
            }

            return new ConnectionInfo(model.IpAddress, model.Port);
        }

        public void SaveConnectionInfo(ConnectionInfo connectionInfo)
        {
            var model = new ConnectionInfoModel(connectionInfo.IpAddress, connectionInfo.Port);

            _connectionStorageProvider.SaveConnectionInfo(model);
        }
    }
}
