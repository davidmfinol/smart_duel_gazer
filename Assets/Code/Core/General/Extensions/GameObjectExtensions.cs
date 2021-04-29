using UnityEngine;

namespace AssemblyCSharp.Assets.Code.Core.General.Extensions
{
    public static class GameObjectExtensions
    {
        public static bool IsClone(this GameObject value)
        {
            return value.name.ToLowerInvariant().Contains("clone");
        }
    }
}
