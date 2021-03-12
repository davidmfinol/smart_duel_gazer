using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Connection.Entities;

namespace AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Connection
{
    public interface IConnectionDataManager
    {
        ConnectionInfo GetConnectionInfo();
        void SaveConnectionInfo(ConnectionInfo connectionInfo);
    }
}
