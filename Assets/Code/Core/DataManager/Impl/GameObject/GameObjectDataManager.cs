using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.GameObject;
using AssemblyCSharp.Assets.Code.Core.Storage.Interface.GameObject;
using Zenject;

namespace AssemblyCSharp.Assets.Code.Core.DataManager.Impl.GameObject
{
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

        public UnityEngine.GameObject GetGameObject(string key)
        {
            return _gameObjectStorageProvider.GetGameObject(key);
        }

        public void SaveGameObject(string key, UnityEngine.GameObject model)
        {
            _gameObjectStorageProvider.SaveGameObject(key, model);
        }

        public void RemoveGameObject(string key)
        {
            _gameObjectStorageProvider.RemoveGameObject(key);
        }

        #endregion

        #region Card model

        public UnityEngine.GameObject GetCardModel(int cardId)
        {
            return _gameObjectStorageProvider.GetCardModel(cardId);
        }

        #endregion
    }
}
