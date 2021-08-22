using Code.Wrappers.WrapperPlayerPrefs;

namespace Code.Core.Storage.Settings
{
    public interface ISettingsStorageProvider
    {
        bool IsDeveloperModeEnabled();
        void SaveDeveloperModeEnabled(bool value);
    }

    public class SettingsStorageProvider : ISettingsStorageProvider
    {
        private const string DeveloperModeEnabledKey = "developerModeEnabled";
        
        private readonly IPlayerPrefsProvider _playerPrefsProvider;

        public SettingsStorageProvider(
            IPlayerPrefsProvider playerPrefsProvider)
        {
            _playerPrefsProvider = playerPrefsProvider;
        }

        public bool IsDeveloperModeEnabled()
        {
            return _playerPrefsProvider.GetBool(DeveloperModeEnabledKey);
        }

        public void SaveDeveloperModeEnabled(bool value)
        {
            _playerPrefsProvider.SetBool(DeveloperModeEnabledKey, value);
        }
    }
}