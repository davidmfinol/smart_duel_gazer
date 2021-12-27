using Code.Core.Config.Entities;
using Code.Core.DataManager.Connections;
using Code.Core.DataManager.Connections.Entities;
using Code.Core.Storage.Connections;
using Code.Core.Storage.Connections.Models;
using Moq;
using NUnit.Framework;

namespace Editor.Tests.EditModeTests.Core.DataManager.Connections
{
    public class ConnectionDataManagerTests
    {
        private IConnectionDataManager _connectionDataManager;

        private Mock<IAppConfig> _appConfig;
        private Mock<IConnectionStorageProvider> _connectionStorageProvider;

        private const string OnlineServerAddress = "http://smart.duel.server";
        private const string OnlineServerPort = "80";
        private const string SecureOnlineServerAddress = "https://smart.duel.server";
        private const string SecureOnlineServerPort = "443";

        private const string LocalServerAddress = "0.0.0.0";
        private const string LocalServerPort = "8080";
        private readonly ConnectionInfo _localConnectionInfo = new ConnectionInfo(LocalServerAddress, LocalServerPort);

        private readonly ConnectionInfoModel _localConnectionInfoModel =
            new ConnectionInfoModel(LocalServerAddress, LocalServerPort);

        [SetUp]
        public void SetUp()
        {
            _appConfig = new Mock<IAppConfig>();
            _connectionStorageProvider = new Mock<IConnectionStorageProvider>();

            _appConfig.SetupGet(ac => ac.SdsAddress).Returns(OnlineServerAddress);
            _appConfig.SetupGet(ac => ac.SdsPort).Returns(OnlineServerPort);
            _appConfig.SetupGet(ac => ac.SecureSdsAddress).Returns(SecureOnlineServerAddress);
            _appConfig.SetupGet(ac => ac.SecureSdsPort).Returns(SecureOnlineServerPort);

            _connectionDataManager = new ConnectionDataManager(
                _appConfig.Object,
                _connectionStorageProvider.Object);
        }

        [TestCase(false, false, false, false, null, null)]
        [TestCase(false, false, false, true, null, null)]
        [TestCase(false, false, true, false, LocalServerAddress, LocalServerPort)]
        [TestCase(false, false, true, true, LocalServerAddress, LocalServerPort)]
        [TestCase(false, true, false, false, null, null)]
        [TestCase(false, true, true, true, LocalServerAddress, LocalServerPort)]
        [TestCase(true, false, false, false, OnlineServerAddress, OnlineServerPort)]
        [TestCase(true, false, false, true, SecureOnlineServerAddress, SecureOnlineServerPort)]
        [TestCase(true, false, true, false, OnlineServerAddress, OnlineServerPort)]
        [TestCase(true, false, true, true, SecureOnlineServerAddress, SecureOnlineServerPort)]
        [TestCase(true, true, false, false, null, null)]
        [TestCase(true, true, false, true, null, null)]
        [TestCase(true, true, true, false, LocalServerAddress, LocalServerPort)]
        [TestCase(true, true, true, true, LocalServerAddress, LocalServerPort)]
        public void When_GetConnectionInfoCalled_Then_ConnectionInfoReturned(bool useOnlineDuelRoom, bool forceLocalInfo,
            bool hasLocalConnectionInfo, bool useSecureConnection, string expectedAddress, string expectedPort)
        {
            // Arrange
            _appConfig.SetupGet(config => config.UseSecureSdsConnection).Returns(useSecureConnection);
            _connectionStorageProvider.Setup(csp => csp.UseOnlineDuelRoom()).Returns(useOnlineDuelRoom);
            _connectionStorageProvider.Setup(csp => csp.GetConnectionInfo())
                .Returns(hasLocalConnectionInfo ? _localConnectionInfoModel : null);

            // Act
            var result = _connectionDataManager.GetConnectionInfo(forceLocalInfo: forceLocalInfo);

            // Assert
            if (expectedAddress == null && expectedPort == null)
            {
                Assert.Null(result);
            }
            else
            {
                Assert.AreEqual(new ConnectionInfo(expectedAddress, expectedPort), result);
            }
        }

        [Test]
        public void When_SaveConnectionInfoCalled_Then_ConnectionInfoSaved()
        {
            // Act
            _connectionDataManager.SaveConnectionInfo(_localConnectionInfo);

            // Assert
            _connectionStorageProvider.Verify(csp => csp.SaveConnectionInfo(_localConnectionInfoModel), Times.Once);
        }

        [Test]
        public void When_UseOnlineDuelRoomCalled_Then_UseOnlineDuelRoomReturned()
        {
            // Arrange
            _connectionStorageProvider.Setup(csp => csp.UseOnlineDuelRoom()).Returns(true);

            // Act
            var result = _connectionDataManager.UseOnlineDuelRoom();

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void When_SaveUseOnlineDuelRoomCalled_Then_UseOnlineDuelRoomSaved()
        {
            // Act
            _connectionDataManager.SaveUseOnlineDuelRoom(true);

            // Assert
            _connectionStorageProvider.Verify(csp => csp.SaveUseOnlineDuelRoom(true), Times.Once);
        }
    }
}