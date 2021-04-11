using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.CardModel;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Connection;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.ModelRecycler;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Connection.Entities;
using UnityEngine;
using Zenject;

namespace AssemblyCSharp.Assets.Code.Core.DataManager.Impl
{
    public class DataManager : IDataManager
    {
        private readonly IConnectionDataManager _connectionDataManager;
        private readonly ICardModelDataManager _cardModelDataManager;
        private readonly IModelRecycler _modelRecycler;

        [Inject]
        public DataManager(
            IConnectionDataManager connectionDataManager,
            ICardModelDataManager cardModelDataManager,
            IModelRecycler modelRecycler)
        {
            _connectionDataManager = connectionDataManager;
            _cardModelDataManager = cardModelDataManager;
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

        #region ModelRecycler

        public void AddToQueue(string key, GameObject model)
        {
            _modelRecycler.AddToQueue(key, model);
        }

        public GameObject GetFromQueue(string key, Vector3 position, Quaternion rotation, Transform parent)
        {
            return _modelRecycler.GetFromQueue(key, position, rotation, parent);
        }

        public bool DoesModelExist(string key)
        {
            return _modelRecycler.DoesModelExist(key);
        }

        public bool DoesPlayfieldExist()
        {
            return _modelRecycler.DoesPlayfieldExist();
        }

        #endregion
    }
}