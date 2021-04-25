using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.CardModel;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Connection;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.ModelRecycler;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Connection.Entities;
using UnityEngine;
using Zenject;
using System.Threading.Tasks;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.CardImage;

namespace AssemblyCSharp.Assets.Code.Core.DataManager.Impl
{
    public class DataManager : IDataManager
    {
        private readonly IConnectionDataManager _connectionDataManager;
        private readonly ICardModelDataManager _cardModelDataManager;
        private readonly ICardImageDataManager _cardImageDataManager;
        private readonly IModelRecycler _modelRecycler;

        [Inject]
        public DataManager(
            IConnectionDataManager connectionDataManager,
            ICardModelDataManager cardModelDataManager,
            ICardImageDataManager cardImageDataManager,
            IModelRecycler modelRecycler)
        {
            _connectionDataManager = connectionDataManager;
            _cardModelDataManager = cardModelDataManager;
            _cardImageDataManager = cardImageDataManager;
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

        #region CardModel

        public GameObject GetCardModel(string cardId)
        {
            return _cardModelDataManager.GetCardModel(cardId);
        }

        #endregion

        #region CardImage

        public Task<Texture> GetCardImage(string cardId)
        {
            return _cardImageDataManager.GetCardImage(cardId);
        }

        #endregion

        #region ModelRecycler

        public void AddGameObjectToQueue(string key, GameObject model)
        {
            _modelRecycler.AddGameObjectToQueue(key, model);
        }

        public GameObject GetGameObjectFromQueue(string key, Vector3 position, Quaternion rotation, Transform parent)
        {
            return _modelRecycler.GetGameObjectFromQueue(key, position, rotation, parent);
        }

        public void RemoveGameObject(string key)
        {
            _modelRecycler.RemoveGameObject(key);
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
