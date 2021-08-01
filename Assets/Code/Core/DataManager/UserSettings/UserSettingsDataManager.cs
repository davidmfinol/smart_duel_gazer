using Code.Core.Storage.UserSettings;
using Zenject;

namespace Code.Core.DataManager.UserSettings
{
    public interface IUserSettingsDataManager
    {
        bool HasKey(string key);
        bool GetBool(string key);
        string GetString(string key);
        void SetBool(string key, bool value);
        void SetString(string key, string value);
    }

    public class UserSettingsDataManager : IUserSettingsDataManager
    {
        private IUserSettingsStorageProvider _userSettingsStorageProvider;

        [Inject]
        public void Construct(IUserSettingsStorageProvider userSettingsStorageProvider)
        {
            _userSettingsStorageProvider = userSettingsStorageProvider;
        }

        public bool HasKey(string key)
        {
            return _userSettingsStorageProvider.HasKey(key);
        }
        
        public string GetString(string key)
        {
            return _userSettingsStorageProvider.GetString(key);
        }

        public void SetString(string key, string value)
        {
            _userSettingsStorageProvider.SetString(key, value);
        }

        public bool GetBool(string key)
        {
            return _userSettingsStorageProvider.GetBool(key);
        }

        public void SetBool(string key, bool value)
        {
            _userSettingsStorageProvider.SetBool(key, value);
        }
    }
}
