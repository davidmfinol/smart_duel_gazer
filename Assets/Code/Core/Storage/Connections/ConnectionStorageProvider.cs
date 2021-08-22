using Code.Core.Storage.Connections.Models;
using Code.Wrappers.WrapperPlayerPrefs;
using Newtonsoft.Json;

namespace Code.Core.Storage.Connections
{
    public interface IConnectionStorageProvider
    {
        ConnectionInfoModel GetConnectionInfo();
        void SaveConnectionInfo(ConnectionInfoModel connectionInfo);
        bool UseOnlineDuelRoom();
        void SaveUseOnlineDuelRoom(bool value);
    }
    
    public class ConnectionStorageProvider : IConnectionStorageProvider
    {
        private const string ConnectionInfoKey = "connectionInfo";
        private const string UseOnlineDuelRoomKey = "useOnlineDuelRoom";

        private readonly IPlayerPrefsProvider _playerPrefsProvider;
        
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
            return JsonConvert.DeserializeObject<ConnectionInfoModel>(json);
        }

        public void SaveConnectionInfo(ConnectionInfoModel connectionInfo)
        {
            var json = JsonConvert.SerializeObject(connectionInfo);
            _playerPrefsProvider.SetString(ConnectionInfoKey, json);
        }

        public bool UseOnlineDuelRoom()
        {
            return _playerPrefsProvider.GetBool(UseOnlineDuelRoomKey);
        }

        public void SaveUseOnlineDuelRoom(bool value)
        {
            _playerPrefsProvider.SetBool(UseOnlineDuelRoomKey, value);
        }
    }
}
