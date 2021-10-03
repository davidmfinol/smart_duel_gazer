using Code.Core.Storage.Settings;

namespace Code.Core.DataManager.Settings
{
    public interface ISettingsDataManager
    {
        bool IsDeveloperModeEnabled();
        void SaveDeveloperModeEnabled(bool value);
    }

    public class SettingsDataManager : ISettingsDataManager
    {
        private readonly ISettingsStorageProvider _settingsStorageProvider;

        public SettingsDataManager(
            ISettingsStorageProvider settingsStorageProvider)
        {
            _settingsStorageProvider = settingsStorageProvider;
        }

        public bool IsDeveloperModeEnabled()
        {
            return _settingsStorageProvider.IsDeveloperModeEnabled();
        }

        public void SaveDeveloperModeEnabled(bool value)
        {
            _settingsStorageProvider.SaveDeveloperModeEnabled(value);
        }
    }
}