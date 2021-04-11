using UnityEngine;

namespace AssemblyCSharp.Assets.Code.Core.DataManager.Interface.ModelRecycler
{
    public interface IModelRecycler
    {
        public void AddToQueue(string key, GameObject model);
        public GameObject GetFromQueue(string key, Vector3 position, Quaternion rotation, Transform parent);
        public bool DoesModelExist(string key);
        public bool CheckForPlayfield();
    }
}
