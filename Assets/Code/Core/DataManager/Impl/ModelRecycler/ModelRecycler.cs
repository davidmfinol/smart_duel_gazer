using UnityEngine;
using System.Collections.Generic;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.ModelRecycler;

namespace AssemblyCSharp.Assets.Core.DataManager.Impl.ModelRecycler
{
    public class ModelRecycler : IModelRecycler
    {
        private readonly Dictionary<string, Queue<GameObject>> _generalRecycler = new Dictionary<string, Queue<GameObject>>();

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
            model.transform.SetParent(parent);
            model.SetActive(true);
            return model;
        }

        public bool DoesModelExist(string key)
        {
            return _generalRecycler.TryGetValue(key, out _);
        }

        public bool CheckForPlayfield()
        {
            return _generalRecycler.TryGetValue("Playfield", out var _);
        }
    }
}
