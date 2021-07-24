using Zenject;
using System.Threading.Tasks;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.GameObject;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Connection;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Connection.Entities;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Texture;
using Code.Core.DataManager.DuelRoom;
using Code.Core.SmartDuelServer.Interface.Entities.EventData.RoomEvents;

namespace AssemblyCSharp.Assets.Code.Core.DataManager.Impl
{
    public class DataManager : IDataManager
    {
        private readonly IConnectionDataManager _connectionDataManager;
        private readonly IGameObjectDataManager _gameObjectDataManager;
        private readonly ITextureDataManager _textureDataManager;
        private readonly IDuelRoomDataManager _duelRoomDataManager;

        [Inject]
        public DataManager(
            IConnectionDataManager connectionDataManager,
            IGameObjectDataManager gameObjectDataManager,
            ITextureDataManager textureDataManager,
            IDuelRoomDataManager duelRoomDataManager)
        {
            _connectionDataManager = connectionDataManager;
            _gameObjectDataManager = gameObjectDataManager;
            _textureDataManager = textureDataManager;
            _duelRoomDataManager = duelRoomDataManager;
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

        public UnityEngine.GameObject GetCardModel(int cardId)
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

        #region Duel room

        public DuelRoom GetDuelRoom()
        {
            return _duelRoomDataManager.GetDuelRoom();
        }

        public void SaveDuelRoom(DuelRoom room)
        {
            _duelRoomDataManager.SaveDuelRoom(room);
        }

        #endregion
    }
}