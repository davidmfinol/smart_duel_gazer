using Code.Wrappers.WrapperPlayerPrefs;
using Zenject;

namespace Code.Core.Storage.UserSettings
{
    public interface IUserSettingsStorageProvider
    {
        bool HasKey(string key);
        bool GetBool(string key);
        string GetString(string key);
        void SetBool(string key, bool value);
        void SetString(string key, string value);
    }

    public class UserSettingsStorageProvider : IUserSettingsStorageProvider
    {
        private IPlayerPrefsProvider _playerPrefsProvider;

        [Inject]
        public void Construct(IPlayerPrefsProvider playerPrefsProvider)
        {
            _playerPrefsProvider = playerPrefsProvider;
        }

        public bool HasKey(string key)
        {
            return _playerPrefsProvider.HasKey(key);
        }
        
        public string GetString(string key)
        {
            return _playerPrefsProvider.GetString(key);
        }

        public void SetString(string key, string value)
        {
            _playerPrefsProvider.SetString(key, value);
        }

        public bool GetBool(string key)
        {
            return _playerPrefsProvider.GetBool(key);
        }

        public void SetBool(string key, bool value)
        {
            _playerPrefsProvider.SetBool(key, value);
        }
    }
}
