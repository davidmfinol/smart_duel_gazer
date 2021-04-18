using AssemblyCSharp.Assets.Code.Core.Screen.Interface;
using UnityEngine;

namespace AssemblyCSharp.Assets.Code.Core.Screen.Impl
{
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
