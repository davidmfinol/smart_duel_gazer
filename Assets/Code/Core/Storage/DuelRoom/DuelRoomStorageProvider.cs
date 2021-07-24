namespace Code.Core.Storage.DuelRoom
{
    public interface IDuelRoomStorageProvider
    {
        SmartDuelServer.Interface.Entities.EventData.RoomEvents.DuelRoom GetDuelRoom();
        void SaveDuelRoom(SmartDuelServer.Interface.Entities.EventData.RoomEvents.DuelRoom room);
    }
    
    public class DuelRoomStorageProvider : IDuelRoomStorageProvider
    {
        private SmartDuelServer.Interface.Entities.EventData.RoomEvents.DuelRoom _room;
        
        public SmartDuelServer.Interface.Entities.EventData.RoomEvents.DuelRoom GetDuelRoom()
        {
            return _room;
        }

        public void SaveDuelRoom(SmartDuelServer.Interface.Entities.EventData.RoomEvents.DuelRoom room)
        {
            _room = room;
        }
    }
}