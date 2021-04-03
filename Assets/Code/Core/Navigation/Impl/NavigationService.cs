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
        
        public void ShowSpeedDuelScene()
        {
            SceneManager.LoadSceneAsync((int)Routes.SpeedDuel);
        }
    }
}
