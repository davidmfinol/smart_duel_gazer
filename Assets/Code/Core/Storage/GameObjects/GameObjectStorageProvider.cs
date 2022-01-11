using System.Collections.Generic;
using System.Linq;
using Code.Wrappers.WrapperResources;
using UnityEngine;
using Zenject;

namespace Code.Core.Storage.GameObjects
{
    public interface IGameObjectStorageProvider
    {
        public GameObject GetGameObject(string key);
        public void SaveGameObject(string key, GameObject go);
        public void RemoveGameObject(string key);
        public GameObject GetCardModel(int cardId);
        public GameObject GetPlayfield();
        public void SavePlayfield(GameObject playfield);
        public void RemovePlayfield();
    }

    public class GameObjectStorageProvider : IGameObjectStorageProvider
    {
        private const string MonsterResourcesPath = "Monsters";

        private readonly IResourcesProvider _resourcesProvider;

        private readonly Dictionary<string, List<GameObject>> _gameObjects = new Dictionary<string, List<GameObject>>();
        private List<GameObject> _cardModels;
        private GameObject _playfield;

        [Inject]
        public GameObjectStorageProvider(
            IResourcesProvider resourcesProvider)
        {
            _resourcesProvider = resourcesProvider;
        }

        #region General

        public GameObject GetGameObject(string key)
        {
            return _gameObjects.ContainsKey(key) ? _gameObjects[key].FirstOrDefault(go => !go.activeInHierarchy) : null;
        }

        public void SaveGameObject(string key, GameObject go)
        {
            if (!_gameObjects.ContainsKey(key))
            {
                _gameObjects.Add(key, new List<GameObject>());
            }

            _gameObjects[key].Add(go);
        }

        public void RemoveGameObject(string key)
        {
            if (_gameObjects.ContainsKey(key))
            {
                _gameObjects.Remove(key);
            }
        }

        #endregion

        #region Card model

        public GameObject GetCardModel(int cardId)
        {
            var modelName = cardId.ToString();

            var model = GetGameObject(modelName);
            if (model != null)
            {
                return model;
            }

            if (_cardModels == null)
            {
                LoadCardModels();
            }

            return _cardModels.FirstOrDefault(cardModel => cardModel.name.Equals(modelName));
        }

        private void LoadCardModels()
        {
            _cardModels = _resourcesProvider.LoadAll<GameObject>(MonsterResourcesPath).ToList();
        }

        #endregion

        #region Playfield

        public GameObject GetPlayfield()
        {
            return _playfield;
        }

        public void SavePlayfield(GameObject playfield)
        {
            _playfield = playfield;
        }

        public void RemovePlayfield()
        {
            _playfield = null;
        }

        #endregion
    }
}