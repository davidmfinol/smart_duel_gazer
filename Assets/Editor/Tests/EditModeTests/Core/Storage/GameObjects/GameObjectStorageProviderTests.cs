using System;
using Code.Core.Storage.GameObjects;
using Code.Wrappers.WrapperResources;
using Moq;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor.Tests.EditModeTests.Core.Storage.GameObjects
{
    public class GameObjectStorageProviderTests
    {
        private IGameObjectStorageProvider _gameObjectStorageProvider;

        private Mock<IResourcesProvider> _resourcesProvider;

        private const string Key = "key";
        private const string MonsterResourcesPath = "Monsters";

        private readonly GameObject _gameObject = new GameObject();

        [SetUp]
        public void SetUp()
        {
            _resourcesProvider = new Mock<IResourcesProvider>();

            _gameObjectStorageProvider = new GameObjectStorageProvider(
                _resourcesProvider.Object);
        }

        [Test]
        public void Given_NoGameObjectFound_When_GetGameObjectCalled_NullReturned()
        {
            var result = _gameObjectStorageProvider.GetGameObject(Key);

            Assert.IsNull(result);
        }

        [Test]
        public void Given_QueueIsEmpty_When_GetGameObjectCalled_NullReturned()
        {
            _gameObjectStorageProvider.SaveGameObject(Key, null);

            var result = _gameObjectStorageProvider.GetGameObject(Key);

            Assert.IsNull(result);
        }

        [Test]
        public void Given_GameObjectExists_When_GetGameObjectCalled_GameObjectReturned()
        {
            var expected = new Object() as UnityEngine.GameObject;
            _gameObjectStorageProvider.SaveGameObject(Key, expected);

            var result = _gameObjectStorageProvider.GetGameObject(Key);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void When_SaveGameObjectCalled_Then_GameObjectSaved()
        {
            _gameObjectStorageProvider.SaveGameObject(Key, _gameObject);
            var result = _gameObjectStorageProvider.GetGameObject(Key);

            Assert.AreEqual(_gameObject, result);
        }

        [Test]
        public void When_RemoveGameObjectCalled_Then_GameObjectRemoved()
        {
            _gameObjectStorageProvider.SaveGameObject(Key, _gameObject);

            _gameObjectStorageProvider.RemoveGameObject(Key);
            var result = _gameObjectStorageProvider.GetGameObject(Key);

            Assert.IsNull(result);
        }

        [Test]
        public void Given_CardModelsLoaded_When_GetCardModelCalled_Then_CardModelReturned()
        {
            _gameObjectStorageProvider.SaveGameObject("123", _gameObject);

            var result = _gameObjectStorageProvider.GetCardModel(123);

            Assert.AreEqual(_gameObject, result);
        }

        [Test]
        public void Given_CardModelsNotLoaded_When_GetCardModelCalled_Then_CardModelsLoaded()
        {
            _gameObjectStorageProvider.GetCardModel(123);

            _resourcesProvider.Verify(rp => rp.LoadAll<GameObject>(MonsterResourcesPath), Times.Once);
        }

        [Test]
        public void Given_CardModelsNotLoaded_And_CardModelDoesNotExist_When_GetCardModelCalled_Then_NullReturned()
        {
            _resourcesProvider.Setup(rp => rp.LoadAll<GameObject>(MonsterResourcesPath)).Returns(Array.Empty<GameObject>());

            var result = _gameObjectStorageProvider.GetCardModel(123);

            Assert.IsNull(result);
        }

        [Test]
        public void Given_CardModelsNotLoaded_And_CardModelExists_When_GetCardModelCalled_Then_CardModelReturned()
        {
            var cardModel = new GameObject
            {
                name = "123"
            };

            _resourcesProvider.Setup(rp => rp.LoadAll<GameObject>(MonsterResourcesPath)).Returns(new[] { cardModel });

            var result = _gameObjectStorageProvider.GetCardModel(123);

            Assert.AreEqual(cardModel, result);
        }

        [Test]
        public void When_SavePlayfieldCalled_Then_PlayfieldSaved()
        {
            _gameObjectStorageProvider.SavePlayfield(_gameObject);

            Assert.AreEqual(_gameObjectStorageProvider.GetPlayfield(), _gameObject);
        }

        [Test]
        public void When_RemovePlayfieldCalled_Then_PlayfieldRemoved()
        {
            _gameObjectStorageProvider.SavePlayfield(_gameObject);

            _gameObjectStorageProvider.RemovePlayfield();

            Assert.IsNull(_gameObjectStorageProvider.GetPlayfield());
        }
    }
}