using System.Collections.Generic;
using System.Linq;
using AssemblyCSharp.Assets.Code.Core.Storage.Impl.Providers.Resources.Interface;
using AssemblyCSharp.Assets.Code.Core.Storage.Interface.GameObject;
using Zenject;

namespace AssemblyCSharp.Assets.Code.Core.Storage.Impl.GameObject
{
    public class GameObjectStorageProvider : IGameObjectStorageProvider
    {
        private const string MonsterResourcesPath = "Monsters";

        private readonly IResourcesProvider _resourcesProvider;

        private readonly Dictionary<string, Queue<UnityEngine.GameObject>> _gameObjects = new Dictionary<string, Queue<UnityEngine.GameObject>>();
        private UnityEngine.GameObject[] _cardModels;

        [Inject]
        public GameObjectStorageProvider(
            IResourcesProvider resourcesProvider)
        {
            _resourcesProvider = resourcesProvider;
        }

        #region General

        public UnityEngine.GameObject GetGameObject(string key)
        {
            if (!_gameObjects.ContainsKey(key) || _gameObjects[key].Count == 0)
            {
                return null;
            }

            return _gameObjects[key].Dequeue();
        }

        public void SaveGameObject(string key, UnityEngine.GameObject model)
        {
            if (!_gameObjects.ContainsKey(key))
            {
                _gameObjects.Add(key, new Queue<UnityEngine.GameObject>());
            }

            _gameObjects[key].Enqueue(model);
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

        public UnityEngine.GameObject GetCardModel(int cardId)
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
            _cardModels = _resourcesProvider.LoadAll<UnityEngine.GameObject>(MonsterResourcesPath);
        }

        #endregion
    }
}
