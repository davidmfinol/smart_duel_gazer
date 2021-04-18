using UnityEngine;
using System.Collections.Generic;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.ModelRecycler;

namespace AssemblyCSharp.Assets.Code.Core.DataManager.Impl.ModelRecycler
{
    public class ModelRecycler : IModelRecycler
    {
        private readonly Dictionary<string, Queue<GameObject>> _generalRecycler = new Dictionary<string, Queue<GameObject>>();
        private readonly Dictionary<string, Texture> _images = new Dictionary<string, Texture>();

        public void AddToQueue(string key, GameObject model)
        {            
            if (!_generalRecycler.ContainsKey(key))
            {
                _generalRecycler.Add(key, new Queue<GameObject>());
            }

            _generalRecycler[key].Enqueue(model);
            model.SetActive(false);
        }

        public GameObject GetFromQueue(string key, Vector3 position, Quaternion rotation, Transform parent)
        {
            var model = _generalRecycler[key].Dequeue();
            model.transform.SetPositionAndRotation(position, rotation);
            model.SetActive(true);
            return model;
        }

        public void Remove(string key)
        {
            _generalRecycler.Remove(key);
        }
        
        public void CacheImage(string key, Texture texture)
        {
            _images.Add(key, texture);
        }

        public Texture GetCachedImage(string key)
        {
            if (!_images.TryGetValue(key, out var texture))
            {
                Debug.LogWarning("Texture Doesn't Exist");
                return null;
            }

            return texture;
        }

        #region Bools

        public bool DoesModelExist(string key)
        {
            return _generalRecycler.TryGetValue(key, out _);
        }

        public bool DoesPlayfieldExist()
        {
            return _generalRecycler.TryGetValue("Playfield", out var _);
        }

        public bool DoesCachedImageExist(string key)
        {
            return _images.TryGetValue(key, out _);
        }

        #endregion

    }
}
