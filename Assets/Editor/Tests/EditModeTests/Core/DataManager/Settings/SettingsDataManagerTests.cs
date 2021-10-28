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

        [TestCase(true)]
        [TestCase(false)]
        public void When_IsDeveloperModeEnabled_Then_SettingIsReturned(bool state)
        {
            _settingsStorageProvider.Setup(ssp => ssp.IsDeveloperModeEnabled()).Returns(state);

            var expected = _settingsDataManager.IsDeveloperModeEnabled();

            Assert.AreEqual(state, expected);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void When_SaveDeveloperModeEnabled_Then_StorageProviderSavesState(bool state)
        {
            _settingsDataManager.SaveDeveloperModeEnabled(state);

            _settingsStorageProvider.Verify(ssp => ssp.SaveDeveloperModeEnabled(state), Times.Once);
        }
    }
}