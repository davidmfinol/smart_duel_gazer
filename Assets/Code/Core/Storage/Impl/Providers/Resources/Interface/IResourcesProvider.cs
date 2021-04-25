using UnityEngine;

namespace AssemblyCSharp.Assets.Code.Core.Storage.Impl.Providers.Resources.Interface
{
    public interface IResourcesProvider
    {
        T[] LoadAll<T>(string path) where T : Object;
    }
}
