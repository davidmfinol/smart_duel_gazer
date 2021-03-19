using UnityEngine;

namespace AssemblyCSharp.Assets.Code.Core.General.Extensions
{
    public static class UtilityExtensions
    {
        public static float MapToNewValue(this float value, 
                                float originalMin, 
                                float originalMax, 
                                float newMin, 
                                float newMax)
        {
            return (value - originalMin) / (originalMax - originalMin) * (newMax - newMin) + newMin;
        }

        public static bool IsWithinRange(this float value, float min, float max)
        {
            return value >= min && value <= max;
        }

        public static void SetRendererVisibility(this SkinnedMeshRenderer[] renderers, bool visibility)
        {
            foreach (SkinnedMeshRenderer item in renderers)
            {
                item.enabled = visibility;
            }
        }
    }

}
