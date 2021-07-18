namespace Code.Core.Storage.DuelRoom
{
    public interface IDuelRoomStorageProvider
    {
        SmartDuelServer.Interface.Entities.EventData.RoomEvent.DuelRoom GetDuelRoom();
        void SaveDuelRoom(SmartDuelServer.Interface.Entities.EventData.RoomEvent.DuelRoom room);
    }
    
    public class DuelRoomStorageProvider : IDuelRoomStorageProvider
    {
        private SmartDuelServer.Interface.Entities.EventData.RoomEvent.DuelRoom _room;
        
        public SmartDuelServer.Interface.Entities.EventData.RoomEvent.DuelRoom GetDuelRoom()
        {
            return _room;
        }

        public void SaveDuelRoom(SmartDuelServer.Interface.Entities.EventData.RoomEvent.DuelRoom room)
        {
            _room = room;
        }
    }
}