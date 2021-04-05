using UnityEngine;

namespace AssemblyCSharp.Assets.Code.Core.DataManager.Interface.ModelRecycler
{
    public interface IModelRecycler
    {
        public void AddToQueue(int key, GameObject model);
        public void AddToQueue(string key, GameObject model);
        public GameObject GetFromQueue(int key, Vector3 position, Quaternion rotation, Transform parent);
        public GameObject GetFromQueue(int key, Transform parent);
        public GameObject GetFromQueue(string key, Transform parent);
        public bool DoesModelExist(string key);
    }
}
