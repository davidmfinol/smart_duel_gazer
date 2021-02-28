using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.CardModel;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Connection;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Connection.Entities;
using UnityEngine;
using Zenject;

namespace AssemblyCSharp.Assets.Code.Core.DataManager.Impl
{
    public class DataManager : IDataManager
    {
        private readonly IConnectionDataManager _connectionDataManager;
        private readonly ICardModelDataManager _cardModelDataManager;

        [Inject]
        public DataManager(
            IConnectionDataManager connectionDataManager,
            ICardModelDataManager cardModelDataManager)
        {
            _connectionDataManager = connectionDataManager;
            _cardModelDataManager = cardModelDataManager;
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
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
