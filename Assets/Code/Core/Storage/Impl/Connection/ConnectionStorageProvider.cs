using AssemblyCSharp.Assets.Code.Core.Storage.Impl.Providers.PlayerPrefs.Interface;
using AssemblyCSharp.Assets.Code.Core.Storage.Interface.Connection;
using AssemblyCSharp.Assets.Code.Core.Storage.Interface.Connection.Models;
using Newtonsoft.Json;
using Zenject;

namespace AssemblyCSharp.Assets.Code.Core.Storage.Impl.Connection
{
    public class ConnectionStorageProvider : IConnectionStorageProvider
    {
        private const string ConnectionInfoKey = "connectionInfo";

        private readonly IPlayerPrefsProvider _playerPrefsProvider;

        [Inject]
        public ConnectionStorageProvider(
            IPlayerPrefsProvider playerPrefsProvider)
        {
            _playerPrefsProvider = playerPrefsProvider;
        }

        public ConnectionInfoModel GetConnectionInfo()
        {
            if (!_playerPrefsProvider.HasKey(ConnectionInfoKey))
            {
                return null;
            }

            var json = _playerPrefsProvider.GetString(ConnectionInfoKey);
            var connectionInfo = JsonConvert.DeserializeObject<ConnectionInfoModel>(json);

            return connectionInfo;
        }

        public void SaveConnectionInfo(ConnectionInfoModel connectionInfo)
        {
            var json = JsonConvert.SerializeObject(connectionInfo);
            _playerPrefsProvider.SetString(ConnectionInfoKey, json);
        }
    }
}
