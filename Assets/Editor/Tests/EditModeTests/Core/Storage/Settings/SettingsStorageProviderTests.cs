using Code.Core.Storage.Settings;
using Code.Wrappers.WrapperPlayerPrefs;
using Moq;
using NUnit.Framework;

namespace Tests.Core.Storage.Settings
{
    public class SettingsStorageProviderTests
    {
        private ISettingsStorageProvider _settingsStorageProvider;

        private Mock<IPlayerPrefsProvider> _playerPrefsProvider;

        private const string DeveloperModeEnabledKey = "developerModeEnabled";

        [SetUp]
        public void SetUp()
        {
            _playerPrefsProvider = new Mock<IPlayerPrefsProvider>();

            _settingsStorageProvider = new SettingsStorageProvider(
                _playerPrefsProvider.Object);
        }

        [Test]
        public void When_IsDeveloperModeEnabledCalled_Then_DeveloperModeEnabledReturned()
        {
            _playerPrefsProvider.Setup(pps => pps.GetBool(DeveloperModeEnabledKey)).Returns(true);

            var result = _settingsStorageProvider.IsDeveloperModeEnabled();

            Assert.True(result);
        }

        [Test]
        public void When_SaveDeveloperModeEnabledCalled_Then_DeveloperModeEnabledSaved()
        {
            _settingsStorageProvider.SaveDeveloperModeEnabled(true);

            _playerPrefsProvider.Verify(pps => pps.SetBool(DeveloperModeEnabledKey, true), Times.Once);
        }
    }
}