using System.Collections;
using System.Collections.Generic;
using Code.Core.DataManager.GameObjects;
using Code.Core.Storage.GameObjects;
using Moq;
using NUnit.Framework;
using UnityEngine;

namespace Editor.Tests.EditModeTests.Core.DataManager.GameObject
{
    public class GameObjectDataManagerTests
    {
        private IGameObjectDataManager _gameObjectDataManager;

        private Mock<IGameObjectStorageProvider> _gameObjectStorageProvider;

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
            string key = "key";
            var expected = new Object() as UnityEngine.GameObject;
            _gameObjectStorageProvider.Setup(sp => sp.GetGameObject(key)).Returns(expected);

            var result = _gameObjectDataManager.GetGameObject(key);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Given_NoGameObject_When_GetGameObjectCalled_Then_ReturnsNull()
        {
            var result = _gameObjectDataManager.GetGameObject("key");

            Assert.IsNull(result);
        }

        [Test]
        public void When_SaveGameObjectCalled_GameObjectIsSaved()
        {
            string key = "key";
            var obj = new Object() as UnityEngine.GameObject;

            _gameObjectDataManager.SaveGameObject(key, obj);

            _gameObjectStorageProvider.Verify(sp => sp.SaveGameObject(key, obj), Times.Once);
        }

        [Test]
        public void When_RemoveGameObjectCalled_GameObjectIsRemoved()
        {
            _gameObjectDataManager.RemoveGameObject("key");

            _gameObjectStorageProvider.Verify(sp => sp.RemoveGameObject("key"), Times.Once);
        }

        [Test]
        public void Given_GameObjectExists_When_GetCardModelCalled_Then_GameObjectReturned()
        {
            int cardId = 123;
            var expected = new Object() as UnityEngine.GameObject;
            _gameObjectStorageProvider.Setup(sp => sp.GetCardModel(cardId)).Returns(expected);

            var result = _gameObjectDataManager.GetCardModel(cardId);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Given_NoGameObject_When_GetCardModelCalled_Then_GameObjectReturned()
        {
            var result = _gameObjectDataManager.GetCardModel(123);

            Assert.IsNull(result);
        }

        [Test]
        public void Given_PlayfieldExists_Then_PlayfieldReturned()
        {
            var expected = new Object() as UnityEngine.GameObject;
            _gameObjectStorageProvider.Setup(sp => sp.Playfield).Returns(expected);

            var result = _gameObjectDataManager.Playfield;

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Given_NoPlayfield_Then_PlayfieldReturnsNull()
        {
            var result = _gameObjectDataManager.Playfield;

            Assert.IsNull(result);
        }

        [Test]
        public void When_SavePlayfieldCalled_PlayfieldIsSaved()
        {
            var obj = new Object() as UnityEngine.GameObject;

            _gameObjectDataManager.SavePlayfield(obj);

            _gameObjectStorageProvider.Verify(sp => sp.SavePlayfield(obj), Times.Once);
        }

        [Test]
        public void When_RemovePlayfieldCalled_PlayfieldIsRemoved()
        {
            _gameObjectDataManager.RemovePlayfield();

            _gameObjectStorageProvider.Verify(sp => sp.RemovePlayfield(), Times.Once);
        }
    }
}