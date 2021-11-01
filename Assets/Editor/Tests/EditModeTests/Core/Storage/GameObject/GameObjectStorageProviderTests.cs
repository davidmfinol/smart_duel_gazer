using System.Collections;
using System.Collections.Generic;
using Code.Core.Storage.GameObjects;
using Code.Wrappers.WrapperResources;
using Moq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Editor.Tests.EditModeTests.Core.Storage.GameObject
{
    public class GameObjectStorageProviderTests
    {
        private IGameObjectStorageProvider _gameObjectStorageProvider;

        private Mock<IResourcesProvider> _resourcesProvider;

        private const string Key = "key";
        private const string MonsterResourcesPath = "Monsters";

        [SetUp]
        public void SetUp()
        {
            _resourcesProvider = new Mock<IResourcesProvider>();

            _gameObjectStorageProvider = new GameObjectStorageProvider(
                _resourcesProvider.Object);
        }

        [Test]
        public void Given_KeyDoesntExist_When_GetGameObjectCalled_NullReturned()
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
        public void Given_AnObjectInQueue_When_GetGameObjectCalled_GameObjectReturned()
        {
            var expected = new Object() as UnityEngine.GameObject;
            _gameObjectStorageProvider.SaveGameObject(Key, expected);

            var result = _gameObjectStorageProvider.GetGameObject(Key);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void When_SaveGameObjectCalled_Then_ObjectIsSaved()
        {
            var expected = new Object() as UnityEngine.GameObject;

            _gameObjectStorageProvider.SaveGameObject(Key, expected);
            var result = _gameObjectStorageProvider.GetGameObject(Key);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void When_RemoveGameObjectCalled_Then_KeyIsRemoved()
        {
            var obj = new Object() as UnityEngine.GameObject;
            _gameObjectStorageProvider.SaveGameObject(Key, obj);

            _gameObjectStorageProvider.RemoveGameObject(Key);
            var result = _gameObjectStorageProvider.GetGameObject(Key);

            Assert.IsNull(result);
        }

        [Test]
        public void Given_ModelExists_When_GetCardModelCalled_Then_ModelReturned()
        {
            var expected = new Object() as UnityEngine.GameObject;
            _gameObjectStorageProvider.SaveGameObject("123", expected);

            var result = _gameObjectStorageProvider.GetCardModel(123);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Given_NoModelExists_When_GetCardModelCalled_Then_ResourcesAreLoaded()
        {
            _gameObjectStorageProvider.GetCardModel(123);

            _resourcesProvider.Verify(rp => rp.LoadAll<UnityEngine.GameObject>(MonsterResourcesPath), Times.Once);
        }

        // TODO: Add Test to check that model is returned after resources are loaded

        [Test]
        public void When_SavePlayfieldCalled_ThenPlayfieldIsSaved()
        {
            var playfield = new Object() as UnityEngine.GameObject;

            _gameObjectStorageProvider.SavePlayfield(playfield);

            Assert.AreEqual(_gameObjectStorageProvider.Playfield, playfield);
        }

        [Test]
        public void When_RemovePlayfieldCalled_Then_PlayfieldIsNull()
        {
            var playfield = new Object() as UnityEngine.GameObject;
            _gameObjectStorageProvider.SavePlayfield(playfield);

            _gameObjectStorageProvider.RemovePlayfield();

            Assert.IsNull(_gameObjectStorageProvider.Playfield);
        }
    }
}