using Code.Core.Navigation.Entities;
using UnityEngine.SceneManagement;

namespace Code.Core.Navigation
{
    public interface INavigationService
    {
        void ShowOnboardingScene();
        void ShowConnectionScene();
        void ShowDuelRoomScene();
        void ShowSpeedDuelScene();
    }
    
    public class NavigationService : INavigationService
    {
        public void ShowOnboardingScene()
        {
            SceneManager.LoadScene((int)Routes.Onboarding);
        }
        
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
