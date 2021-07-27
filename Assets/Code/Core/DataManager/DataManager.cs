using System.Threading.Tasks;
using Code.Core.DataManager.Connections;
using Code.Core.DataManager.Connections.Entities;
using Code.Core.DataManager.DuelRooms;
using Code.Core.DataManager.GameObjects;
using Code.Core.DataManager.Textures;
using Code.Core.SmartDuelServer.Entities.EventData.RoomEvents;
using UnityEngine;
using Zenject;

namespace Code.Core.DataManager
{
    public interface IDataManager : IConnectionDataManager, IGameObjectDataManager, ITextureDataManager, IDuelRoomDataManager
    {
    }
    
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

        public GameObject GetGameObject(string key)
        {
            return _gameObjectDataManager.GetGameObject(key);
        }

        public void SaveGameObject(string key, GameObject model)
        {
            _gameObjectDataManager.SaveGameObject(key, model);
        }

        public void RemoveGameObject(string key)
        {
            _gameObjectDataManager.RemoveGameObject(key);
        }

        public GameObject GetCardModel(int cardId)
        {
            return _gameObjectDataManager.GetCardModel(cardId);
        }

        #endregion

        #region Texture

        public Task<Texture> GetCardImage(string cardId)
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