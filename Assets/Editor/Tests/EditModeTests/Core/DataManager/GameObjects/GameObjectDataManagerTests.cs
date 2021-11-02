using Code.Core.DataManager.GameObjects;
using Code.Core.Storage.GameObjects;
using Moq;
using NUnit.Framework;
using UnityEngine;

namespace Editor.Tests.EditModeTests.Core.DataManager.GameObjects
{
    public class GameObjectDataManagerTests
    {
        private IGameObjectDataManager _gameObjectDataManager;

        private Mock<IGameObjectStorageProvider> _gameObjectStorageProvider;

        private const string Key = "key";
        
        private readonly GameObject _gameObject = new GameObject();

        [SetUp]
        public void SetUp()
        {
            _gameObjectStorageProvider = new Mock<IGameObjectStorageProvider>();

            _gameObjectDataManager = new GameObjectDataManager(
                _gameObjectStorageProvider.Object);
        }

        [Test]
        public void Given_GameObjectExists_When_GetGameObjectCalled_Then_GameObjectReturned()
        {
            _gameObjectStorageProvider.Setup(sp => sp.GetGameObject(Key)).Returns(_gameObject);

            var result = _gameObjectDataManager.GetGameObject(Key);

            Assert.AreEqual(_gameObject, result);
        }

        [Test]
        public void Given_NoGameObject_When_GetGameObjectCalled_Then_NullReturned()
        {
            var result = _gameObjectDataManager.GetGameObject(Key);

            Assert.IsNull(result);
        }

        [Test]
        public void When_SaveGameObjectCalled_GameObjectSaved()
        {
            _gameObjectDataManager.SaveGameObject(Key, _gameObject);

            _gameObjectStorageProvider.Verify(sp => sp.SaveGameObject(Key, _gameObject), Times.Once);
        }

        [Test]
        public void When_RemoveGameObjectCalled_GameObjectRemoved()
        {
            _gameObjectDataManager.RemoveGameObject(Key);

            _gameObjectStorageProvider.Verify(sp => sp.RemoveGameObject(Key), Times.Once);
        }

        [Test]
        public void Given_GameObjectExists_When_GetCardModelCalled_Then_GameObjectReturned()
        {
            const int cardId = 123;
            _gameObjectStorageProvider.Setup(sp => sp.GetCardModel(cardId)).Returns(_gameObject);

            var result = _gameObjectDataManager.GetCardModel(cardId);

            Assert.AreEqual(_gameObject, result);
        }

        [Test]
        public void Given_NoGameObject_When_GetCardModelCalled_Then_NullReturned()
        {
            var result = _gameObjectDataManager.GetCardModel(123);

            Assert.IsNull(result);
        }

        [Test]
        public void Given_PlayfieldExists_When_GetPlayfieldCalled_Then_PlayfieldReturned()
        {
            _gameObjectStorageProvider.Setup(sp => sp.GetPlayfield()).Returns(_gameObject);

            var result = _gameObjectDataManager.GetPlayfield();

            Assert.AreEqual(_gameObject, result);
        }

        [Test]
        public void Given_NoPlayfield_When_GetPlayfieldCalled_Then_NullReturned()
        {
            var result = _gameObjectDataManager.GetPlayfield();

            Assert.IsNull(result);
        }

        [Test]
        public void When_SavePlayfieldCalled_PlayfieldSaved()
        {
            _gameObjectDataManager.SavePlayfield(_gameObject);

            _gameObjectStorageProvider.Verify(sp => sp.SavePlayfield(_gameObject), Times.Once);
        }

        [Test]
        public void When_RemovePlayfieldCalled_PlayfieldRemoved()
        {
            _gameObjectDataManager.RemovePlayfield();

            _gameObjectStorageProvider.Verify(sp => sp.RemovePlayfield(), Times.Once);
        }
    }
}