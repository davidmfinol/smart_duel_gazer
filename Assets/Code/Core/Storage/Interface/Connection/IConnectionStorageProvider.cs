using AssemblyCSharp.Assets.Code.Core.Storage.Interface.Connection.Models;

namespace AssemblyCSharp.Assets.Code.Core.Storage.Interface.Connection
{
    public interface IConnectionStorageProvider
    {
        ConnectionInfoModel GetConnectionInfo();
        void SaveConnectionInfo(ConnectionInfoModel connectionInfo);
    }
}
