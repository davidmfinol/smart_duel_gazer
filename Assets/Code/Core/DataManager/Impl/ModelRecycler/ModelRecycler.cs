using UnityEngine;
using System.Collections.Generic;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.ModelRecycler;

namespace AssemblyCSharp.Assets.Code.Core.DataManager.Impl.ModelRecycler
{
    public class ModelRecycler : IModelRecycler
    {
        private readonly Dictionary<string, Queue<GameObject>> _gameObjects = new Dictionary<string, Queue<GameObject>>();
        private readonly Dictionary<string, Texture> _images = new Dictionary<string, Texture>();
       
        public bool IsGameObjectRecyclable(string key)
        {
            return _gameObjects.ContainsKey(key);
        }

        public bool IsPlayfieldRecyclable()
        {
            return IsGameObjectRecyclable("Playfield");
        }

        public void AddGameObjectToQueue(string key, GameObject model)
        {
            if (!_gameObjects.ContainsKey(key))
            {
                _gameObjects.Add(key, new Queue<GameObject>());
            }

            _gameObjects[key].Enqueue(model);
            model.SetActive(false);
        }

        public GameObject GetGameObjectFromQueue(string key, Vector3 position, Quaternion rotation, Transform parent)
        {
            var model = _gameObjects[key].Dequeue();
            model.transform.SetPositionAndRotation(position, rotation);
            model.SetActive(true);
            return model;
        }

        public void RemoveGameObject(string key)
        {
            _gameObjects.Remove(key);
        }

        public bool IsImageRecyclable(string key)
        {
            return _images.ContainsKey(key);
        }

        public void SaveImage(string key, Texture texture)
        {
            _images.Add(key, texture);
        }

        public Texture GetImage(string key)
        {
            if (!_images.TryGetValue(key, out var texture))
            {
                Debug.LogWarning("Texture Doesn't Exist");
                return null;
            }

            return texture;
        }
    }
}
