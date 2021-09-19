using UnityEngine.UI;
using Code.Core.SmartDuelServer.Entities.EventData.RoomEvents;
using System.Linq;

namespace Code.Features.DuelRoom
{
    public static class DuelRoomViewHelpers
    {
        public static void ResetDropdown(this Dropdown dropdown, RoomEventData data)
        {
            dropdown.ClearOptions();
            if (data == null) return;

            var options = data.DuelistsIds.ToList();
            dropdown.AddOptions(options);
        }
    }
}