using Code.Core.DataManager.Settings;
using Code.Core.Storage.Settings;
using Moq;
using NUnit.Framework;

namespace Editor.Tests.EditModeTests.Core.DataManager.Settings
{
    public class SettingsDataManagerTests
    {
        private Mock<ISettingsStorageProvider> _settingsStorageProvider;

        private SettingsDataManager _settingsDataManager;

        [SetUp]
        public void Setup()
        {
            _settingsStorageProvider = new Mock<ISettingsStorageProvider>();

            _settingsDataManager = new SettingsDataManager(
                _settingsStorageProvider.Object);
        }

        [Test]
        public void When_IsDeveloperModeEnabledCalled_Then_SettingValueReturned()
        {
            const bool expected = true;
            _settingsStorageProvider.Setup(ssp => ssp.IsDeveloperModeEnabled()).Returns(expected);

            var result = _settingsDataManager.IsDeveloperModeEnabled();

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void When_SaveDeveloperModeEnabledCalled_Then_SettingValueSaved()
        {
            const bool developerModeEnabled = true;
            
            _settingsDataManager.SaveDeveloperModeEnabled(developerModeEnabled);

            _settingsStorageProvider.Verify(ssp => ssp.SaveDeveloperModeEnabled(developerModeEnabled), Times.Once);
        }
    }
}