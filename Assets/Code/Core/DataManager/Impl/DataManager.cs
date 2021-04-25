using Zenject;
using System.Threading.Tasks;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.GameObject;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Connection;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.ModelRecycler;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Connection.Entities;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Texture;
using UnityEngine;

namespace AssemblyCSharp.Assets.Code.Core.DataManager.Impl
{
    public class DataManager : IDataManager
    {
        private readonly IConnectionDataManager _connectionDataManager;
        private readonly IGameObjectDataManager _gameObjectDataManager;
        private readonly ITextureDataManager _textureDataManager;
        private readonly IModelRecycler _modelRecycler;

        [Inject]
        public DataManager(
            IConnectionDataManager connectionDataManager,
            IGameObjectDataManager gameObjectDataManager,
            ITextureDataManager textureDataManager,
            IModelRecycler modelRecycler)
        {
            _connectionDataManager = connectionDataManager;
            _gameObjectDataManager = gameObjectDataManager;
            _textureDataManager = textureDataManager;
            _modelRecycler = modelRecycler;
        }

        #region Connection

        public ConnectionInfo GetConnectionInfo()
        {
            return _connectionDataManager.GetConnectionInfo();
        }

        public void SaveConnectionInfo(ConnectionInfo connectionInfo)
        {
            _connectionDataManager.SaveConnectionInfo(connectionInfo);
        }

        #endregion

        #region Game object

        public UnityEngine.GameObject GetGameObject(string key)
        {
            return _gameObjectDataManager.GetGameObject(key);
        }

        public void SaveGameObject(string key, UnityEngine.GameObject model)
        {
            _gameObjectDataManager.SaveGameObject(key, model);
        }

        public void RemoveGameObject(string key)
        {
            _gameObjectDataManager.RemoveGameObject(key);
        }

        public UnityEngine.GameObject GetCardModel(string cardId)
        {
            return _gameObjectDataManager.GetCardModel(cardId);
        }

        #endregion

        #region Texture

        public Task<UnityEngine.Texture> GetCardImage(string cardId)
        {
            return _textureDataManager.GetCardImage(cardId);
        }

        #endregion

        #region ModelRecycler

        public void AddGameObjectToQueue(string key, UnityEngine.GameObject model)
        {
            _modelRecycler.AddGameObjectToQueue(key, model);
        }

        public UnityEngine.GameObject GetGameObjectFromQueue(string key, Vector3 position, Quaternion rotation, Transform parent)
        {
            return _modelRecycler.GetGameObjectFromQueue(key, position, rotation, parent);
        }

        public void RemoveGameObjectQueue(string key)
        {
            _modelRecycler.RemoveGameObjectQueue(key);
        }

        public bool IsGameObjectRecyclable(string key)
        {
            return _modelRecycler.IsGameObjectRecyclable(key);
        }

        public bool IsPlayfieldRecyclable()
        {
            return _modelRecycler.IsPlayfieldRecyclable();
        }

        #endregion
    }
}
