using UnityEngine;

namespace AssemblyCSharp.Assets.Code.Core.General.Extensions
{
    public static class UtilityExtensions
    {
        public static bool IsWithinRange(this float value, float min, float max)
        {
            return value >= min && value <= max;
        }
        
        public static int StringToInt(this string value, char splitCharacter = '(', int index = 0)
        {
            return int.Parse(value.Split(splitCharacter)[index]);
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
