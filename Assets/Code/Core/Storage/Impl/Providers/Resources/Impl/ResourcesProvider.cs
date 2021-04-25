using AssemblyCSharp.Assets.Code.Core.Storage.Impl.Providers.Resources.Interface;
using UnityEngine;

namespace AssemblyCSharp.Assets.Code.Core.Storage.Impl.Providers.Resources.Impl
{
    public class ResourcesProvider : IResourcesProvider
    {
        public T[] LoadAll<T>(string path) where T : Object
        {
            return UnityEngine.Resources.LoadAll<T>(path);
        }
    }
}
