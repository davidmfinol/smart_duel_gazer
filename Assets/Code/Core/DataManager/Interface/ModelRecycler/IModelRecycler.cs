using UnityEngine;

namespace AssemblyCSharp.Assets.Code.Core.DataManager.Interface.ModelRecycler
{
    public interface IModelRecycler
    {
        public void AddToQueue(string key, GameObject model);
        public GameObject GetFromQueue(string key, Vector3 position, Quaternion rotation, Transform parent);
        public void Remove(string key);
        public void CacheImage(string key, Texture texture);
        public Texture GetCachedImage(string key);
        public bool DoesPlayfieldExist();
        public bool IsModelRecyclable(string key);
        public bool DoesCachedImageExist(string key);
    }
}
