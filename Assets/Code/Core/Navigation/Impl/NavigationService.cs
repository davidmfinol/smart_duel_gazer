using AssemblyCSharp.Assets.Code.Core.Navigation.Interface;
using AssemblyCSharp.Assets.Code.Core.Navigation.Interface.Entities;
using UnityEngine.SceneManagement;

namespace AssemblyCSharp.Assets.Code.Core.Navigation.Impl
{
    public class NavigationService : INavigationService
    {
        public void ShowConnectionScene()
        {
            SceneManager.LoadScene((int)Routes.Connection);
        }

        public void ShowDuelRoomScene()
        {
            SceneManager.LoadScene((int)Routes.DuelRoom);
        }

        public void ShowSpeedDuelScene()
        {
            SceneManager.LoadScene((int)Routes.SpeedDuel);
        }
    }
}
