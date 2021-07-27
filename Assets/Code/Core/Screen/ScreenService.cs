using UnityEngine;

namespace Code.Core.Screen
{
    public interface IScreenService
    {
        void UseAutoOrientation();
        void UsePortraitOrientation();
        void UseAutoLandscapeOrientation();
    }
    
    public class ScreenService : IScreenService
    {
        public void UseAutoOrientation()
        {
            UnityEngine.Screen.orientation = ScreenOrientation.AutoRotation;
        }

        public void UsePortraitOrientation()
        {
            UnityEngine.Screen.orientation = ScreenOrientation.Portrait;
        }

        public void UseAutoLandscapeOrientation()
        {
            UnityEngine.Screen.orientation = ScreenOrientation.Landscape;
            
            UnityEngine.Screen.autorotateToLandscapeLeft = true;
            UnityEngine.Screen.autorotateToLandscapeRight = true;
            UnityEngine.Screen.autorotateToPortrait = false;
            UnityEngine.Screen.autorotateToPortraitUpsideDown = false;

            UnityEngine.Screen.orientation = ScreenOrientation.AutoRotation;
        }
    }
}
