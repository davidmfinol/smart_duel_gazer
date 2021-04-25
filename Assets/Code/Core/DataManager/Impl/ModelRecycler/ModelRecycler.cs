using UnityEngine;
using System.Collections.Generic;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.ModelRecycler;

namespace AssemblyCSharp.Assets.Code.Core.DataManager.Impl.ModelRecycler
{
    public class ModelRecycler : IModelRecycler
    {
        private readonly Dictionary<string, Queue<UnityEngine.GameObject>> _gameObjects = new Dictionary<string, Queue<UnityEngine.GameObject>>();

        public bool IsGameObjectRecyclable(string key)
        {
            return _gameObjects.ContainsKey(key);
        }

        public bool IsPlayfieldRecyclable()
        {
            return IsGameObjectRecyclable("Playfield");
        }

        public void AddGameObjectToQueue(string key, UnityEngine.GameObject model)
        {
            if (!_gameObjects.ContainsKey(key))
            {
                _gameObjects.Add(key, new Queue<UnityEngine.GameObject>());
            }

            _gameObjects[key].Enqueue(model);
            model.SetActive(false);
        }

        public UnityEngine.GameObject GetGameObjectFromQueue(string key, Vector3 position, Quaternion rotation, Transform parent)
        {
            var model = _gameObjects[key].Dequeue();
            model.transform.SetPositionAndRotation(position, rotation);
            model.SetActive(true);
            return model;
        }

        public void RemoveGameObjectQueue(string key)
        {
            _gameObjects.Remove(key);
        }
    }
}
