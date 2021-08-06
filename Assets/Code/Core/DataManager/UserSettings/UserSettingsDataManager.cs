using Code.Core.Storage.UserSettings;

namespace Code.Core.DataManager.UserSettings
{
    public interface IUserSettingsDataManager
    {
        void SetToggleSetting(string settingName, bool value);
        bool IsToggleSettingEnabled(string settingName);
    }

    public class UserSettingsDataManager : IUserSettingsDataManager
    {
        private IUserSettingsStorageProvider _userSettingsStorageProvider;

        public UserSettingsDataManager(
            IUserSettingsStorageProvider userSettingsStorageProvider)
        {
            _userSettingsStorageProvider = userSettingsStorageProvider;
        }

        public void SetToggleSetting(string settingName, bool value)
        {
            _userSettingsStorageProvider.SetBool(settingName, value);
        }

        public bool IsToggleSettingEnabled(string settingName)
        {
            if (!_userSettingsStorageProvider.HasKey(settingName))
            {
                return false;
            }

            return _userSettingsStorageProvider.GetBool(settingName);
        }
    }
}
