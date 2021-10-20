using Code.Core.Storage.Connections;
using Code.Core.Storage.Connections.Models;
using Code.Wrappers.WrapperPlayerPrefs;
using Moq;
using NUnit.Framework;

namespace Editor.Tests.EditModeTests.Core.Storage.Connections
{
    public class ConnectionStorageProviderTests
    {
        private IConnectionStorageProvider _connectionStorageProvider;

        private Mock<IPlayerPrefsProvider> _playerPrefsProvider;

        private const string ConnectionInfoKey = "connectionInfo";
        private const string UseOnlineDuelRoomKey = "useOnlineDuelRoom";

        private const string IpAddress = "0.0.0.0";
        private const string Port = "8080";
        private readonly ConnectionInfoModel _connectionInfoModel = new ConnectionInfoModel(IpAddress, Port);
        private readonly string _connectionInfoModelJson = $"{{\"IpAddress\":\"{IpAddress}\",\"Port\":\"{Port}\"}}";

        [SetUp]
        public void SetUp()
        {
            _playerPrefsProvider = new Mock<IPlayerPrefsProvider>();

            _connectionStorageProvider = new ConnectionStorageProvider(
                _playerPrefsProvider.Object);
        }

        [Test]
        public void Given_NoConnectionInfoAvailable_When_GetConnectionInfoCalled_Then_NullReturned()
        {
            // Arrange
            _playerPrefsProvider.Setup(pps => pps.HasKey(ConnectionInfoKey)).Returns(false);
            
            // Act
            var result = _connectionStorageProvider.GetConnectionInfo();
            
            // Assert
            Assert.Null(result);
        }

        [Test]
        public void Given_ConnectionInfoAvailable_When_GetConnectionInfoCalled_Then_ConnectionInfoReturned()
        {
            // Arrange
            _playerPrefsProvider.Setup(pps => pps.HasKey(ConnectionInfoKey)).Returns(true);
            _playerPrefsProvider.Setup(pps => pps.GetString(ConnectionInfoKey)).Returns(_connectionInfoModelJson);
            
            // Act
            var result = _connectionStorageProvider.GetConnectionInfo();
            
            // Assert
            Assert.AreEqual(_connectionInfoModel, result);
        }

        [Test]
        public void When_SaveConnectionInfoCalled_Then_ConnectionInfoSaved()
        {
            // Act
            _connectionStorageProvider.SaveConnectionInfo(_connectionInfoModel);

            // Assert
            _playerPrefsProvider.Verify(pps => pps.SetString(ConnectionInfoKey, _connectionInfoModelJson));
        }

        [Test]
        public void When_UseOnlineDuelRoomCalled_Then_UseOnlineDuelRoomReturned()
        {
            // Arrange
            _playerPrefsProvider.Setup(pps => pps.GetBool(UseOnlineDuelRoomKey)).Returns(true);

            // Act
            var result = _connectionStorageProvider.UseOnlineDuelRoom();

            // Assert
            Assert.True(result);
        }

        [Test]
        public void When_SaveUseOnlineDuelRoomCalled_Then_UseOnlineDuelRoomSaved()
        {
            // Act
            _connectionStorageProvider.SaveUseOnlineDuelRoom(true);

            // Assert
            _playerPrefsProvider.Verify(pps => pps.SetBool(UseOnlineDuelRoomKey, true));
        }
    }
}