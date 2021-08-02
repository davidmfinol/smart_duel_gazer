using Code.Core.DataManager.Connections;
using Code.Core.DataManager.Connections.Entities;
using Code.Core.Storage.Connection;
using Code.Core.Storage.Connection.Models;
using Moq;
using NUnit.Framework;

namespace Tests.Core.DataManager.Connections
{
    public class ConnectionDataManagerTests
    {
        private IConnectionDataManager _connectionDataManager;

        private Mock<IConnectionStorageProvider> _connectionStorageProvider;

        [SetUp]
        public void SetUp()
        {
            _connectionStorageProvider = new Mock<IConnectionStorageProvider>();

            _connectionDataManager = new ConnectionDataManager(
                _connectionStorageProvider.Object);
        }

        [Test]
        public void Given_NoCachedConnectionInfoAvailable_When_GetConnectionInfoCalled_Then_NullReturned()
        {
            // Arrange
            _connectionStorageProvider.Setup(csp => csp.GetConnectionInfo()).Returns((ConnectionInfoModel) null);

            // Act
            var result = _connectionDataManager.GetConnectionInfo();

            // Assert
            Assert.Null(result);
        }

        [Test]
        public void Given_CachedConnectionInfoAvailable_When_GetConnectionInfoCalled_Then_ConnectionInfoReturned()
        {
            // Arrange
            const string ipAddress = "0.0.0.0";
            const string port = "8080";

            var model = new ConnectionInfoModel(ipAddress, port);
            var expected = new ConnectionInfo(ipAddress, port);
            
            _connectionStorageProvider.Setup(csp => csp.GetConnectionInfo()).Returns(model);

            // Act
            var result = _connectionDataManager.GetConnectionInfo();

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void When_SaveConnectionInfoCalled_Then_ConnectionInfoSaved()
        {
            // Arrange
            const string ipAddress = "0.0.0.0";
            const string port = "8080";
            
            var param = new ConnectionInfo(ipAddress, port);
            var expected = new ConnectionInfoModel(ipAddress, port);
            
            // Act
            _connectionDataManager.SaveConnectionInfo(param);
            
            // Assert
            _connectionStorageProvider.Verify(csp => csp.SaveConnectionInfo(expected), Times.Once);
        }
    }
}