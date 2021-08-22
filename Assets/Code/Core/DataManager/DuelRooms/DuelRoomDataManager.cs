using Code.Core.SmartDuelServer.Entities.EventData.RoomEvents;
using Code.Core.Storage.DuelRooms;
using Zenject;

namespace Code.Core.DataManager.DuelRooms
{
    public interface IDuelRoomDataManager
    {
        DuelRoom GetDuelRoom();
        void SaveDuelRoom(DuelRoom room);
    }

    public class DuelRoomDataManager : IDuelRoomDataManager
    {
        private readonly IDuelRoomStorageProvider _duelRoomStorageProvider;

        [Inject]
        public DuelRoomDataManager(
            IDuelRoomStorageProvider duelRoomStorageProvider)
        {
            _duelRoomStorageProvider = duelRoomStorageProvider;
        }

        public DuelRoom GetDuelRoom()
        {
            return _duelRoomStorageProvider.GetDuelRoom();
        }

        public void SaveDuelRoom(DuelRoom room)
        {
            _duelRoomStorageProvider.SaveDuelRoom(room);
        }
    }
}