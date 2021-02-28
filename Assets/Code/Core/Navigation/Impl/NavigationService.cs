using AssemblyCSharp.Assets.Code.Core.Navigation.Interface;
using AssemblyCSharp.Assets.Code.Core.Navigation.Interface.Entities;
using UnityEngine.SceneManagement;

namespace AssemblyCSharp.Assets.Code.Core.Navigation.Impl
{
    public class NavigationService : INavigationService
    {
        public void ShowSpeedDuelScene()
        {
            SceneManager.LoadScene((int)Routes.SpeedDuel);
        }
    }
}
