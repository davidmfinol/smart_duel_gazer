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
    }
}
