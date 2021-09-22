using Code.Core.Config.Entities;
using Code.Core.DataManager.Connections;
using Code.Core.DataManager.Connections.Entities;
using Code.Core.Storage.Connections;
using Code.Core.Storage.Connections.Models;
using Moq;
using NUnit.Framework;

namespace Tests.Core.DataManager.Connections
{
    public class ConnectionDataManagerTests
    {
        private IConnectionDataManager _connectionDataManager;

        private Mock<IAppConfig> _appConfig;
        private Mock<IConnectionStorageProvider> _connectionStorageProvider;

        private const string OnlineServerAddress = "https://smart.duel.server";
        private const string OnlineServerPort = "123";

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

            _appConfig.SetupGet(ac => ac.SmartDuelServerAddress).Returns(OnlineServerAddress);
            _appConfig.SetupGet(ac => ac.SmartDuelServerPort).Returns(OnlineServerPort);

            _connectionDataManager = new ConnectionDataManager(
                _appConfig.Object,
                _connectionStorageProvider.Object);
        }

        [TestCase(false, false, false, null, null)]
        [TestCase(false, false, true, LocalServerAddress, LocalServerPort)]
        [TestCase(false, true, false, null, null)]
        [TestCase(false, true, true, LocalServerAddress, LocalServerPort)]
        [TestCase(true, false, false, OnlineServerAddress, OnlineServerPort)]
        [TestCase(true, false, true, OnlineServerAddress, OnlineServerPort)]
        [TestCase(true, true, false, null, null)]
        [TestCase(true, true, true, LocalServerAddress, LocalServerPort)]
        public void When_GetConnectionInfoCalled_Then_ConnectionInfoReturned(bool useOnlineDuelRoom, bool forceLocalInfo,
            bool hasLocalConnectionInfo, string expectedAddress, string expectedPort)
        {
            // Arrange
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