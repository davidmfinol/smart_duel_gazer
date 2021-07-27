using UnityEngine;

namespace Code.Wrappers.WrapperResources
{
    public interface IResourcesProvider
    {
        T[] LoadAll<T>(string path) where T : Object;
    }
    
    public class ResourcesProvider : IResourcesProvider
    {
        public T[] LoadAll<T>(string path) where T : Object
        {
            return Resources.LoadAll<T>(path);
        }
    }
}