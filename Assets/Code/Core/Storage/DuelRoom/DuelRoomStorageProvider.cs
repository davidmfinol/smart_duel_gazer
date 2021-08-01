namespace Code.Core.Storage.DuelRoom
{
    public interface IDuelRoomStorageProvider
    {
        SmartDuelServer.Entities.EventData.RoomEvents.DuelRoom GetDuelRoom();
        void SaveDuelRoom(SmartDuelServer.Entities.EventData.RoomEvents.DuelRoom room);
    }
    
    public class DuelRoomStorageProvider : IDuelRoomStorageProvider
    {
        private SmartDuelServer.Entities.EventData.RoomEvents.DuelRoom _room;
        
        public SmartDuelServer.Entities.EventData.RoomEvents.DuelRoom GetDuelRoom()
        {
            return _room;
        }

        public void SaveDuelRoom(SmartDuelServer.Entities.EventData.RoomEvents.DuelRoom room)
        {
            _room = room;
        }
    }
}