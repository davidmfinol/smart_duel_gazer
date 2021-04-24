using UnityEngine;

namespace AssemblyCSharp.Assets.Code.Core.DataManager.Interface.ModelRecycler
{
    public interface IModelRecycler
    {
        public bool IsGameObjectRecyclable(string key);
        public bool IsPlayfieldRecyclable();
        public void AddGameObjectToQueue(string key, GameObject model);
        public GameObject GetGameObjectFromQueue(string key, Vector3 position, Quaternion rotation, Transform parent);
        public void RemoveGameObject(string key);
        public bool IsImageRecyclable(string key);
        public void SaveImage(string key, Texture texture);
        public Texture GetImage(string key);
    }
}
