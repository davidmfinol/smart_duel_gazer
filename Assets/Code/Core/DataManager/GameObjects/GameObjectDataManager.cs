using Code.Core.Storage.GameObjects;
using UnityEngine;
using Zenject;

namespace Code.Core.DataManager.GameObjects
{
    public interface IGameObjectDataManager
    {
        public GameObject GetGameObject(string key);
        public void SaveGameObject(string key, GameObject model);
        public void RemoveGameObject(string key);
        GameObject GetCardModel(int cardId);
        public GameObject GetPlayfield();
        public void SavePlayfield(GameObject playField);
        public void RemovePlayfield();
    }

    public class GameObjectDataManager : IGameObjectDataManager
    {
        private readonly IGameObjectStorageProvider _gameObjectStorageProvider;

        [Inject]
        public GameObjectDataManager(
            IGameObjectStorageProvider gameObjectStorageProvider)
        {
            _gameObjectStorageProvider = gameObjectStorageProvider;
        }

        #region General

        public GameObject GetGameObject(string key)
        {
            return _gameObjectStorageProvider.GetGameObject(key);
        }

        public void SaveGameObject(string key, GameObject model)
        {
            _gameObjectStorageProvider.SaveGameObject(key, model);
        }

        public void RemoveGameObject(string key)
        {
            _gameObjectStorageProvider.RemoveGameObject(key);
        }

        #endregion

        #region Card model

        public GameObject GetCardModel(int cardId)
        {
            return _gameObjectStorageProvider.GetCardModel(cardId);
        }

        #endregion

        #region Playfield

        public GameObject GetPlayfield()
        {
            return _gameObjectStorageProvider.GetPlayfield();
        }

        public void SavePlayfield(GameObject playField)
        {
            _gameObjectStorageProvider.SavePlayfield(playField);
        }

        public void RemovePlayfield()
        {
            _gameObjectStorageProvider.RemovePlayfield();
        }

        #endregion
    }
}