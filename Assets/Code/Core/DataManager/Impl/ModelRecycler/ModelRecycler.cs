using UnityEngine;
using System.Collections.Generic;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.ModelRecycler;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.ModelRecycler.Entities;

namespace AssemblyCSharp.Assets.Core.DataManager.Impl.ModelRecycler
{
    public class ModelRecycler : IModelRecycler
    {
        private readonly Dictionary<int, Queue<GameObject>> _generalRecycler = new Dictionary<int, Queue<GameObject>>();

        #region AddToQueue Overloads

        public void AddToQueue(int key, GameObject model)
        {
            if (!_generalRecycler.ContainsKey(key))
            {
                _generalRecycler.Add(key, new Queue<GameObject>());
            }
            _generalRecycler[key].Enqueue(model);
            model.SetActive(false);
        }
        public void AddToQueue(string stringKey, GameObject model)
        {
            int key = stringKey.StringToInt();
            
            if (!_generalRecycler.ContainsKey(key))
            {
                _generalRecycler.Add(key, new Queue<GameObject>());
            }

            _generalRecycler[key].Enqueue(model);
            model.SetActive(false);
        }

        #endregion

        #region GetFromQueue Overloads

        public GameObject GetFromQueue(int key, Vector3 position, Quaternion rotation, Transform parent)
        {
            var model = _generalRecycler[key].Dequeue();
            model.transform.SetPositionAndRotation(position, rotation);
            model.transform.SetParent(parent);
            model.SetActive(true);
            return model;
        }
        public GameObject GetFromQueue(int key, Transform parent)
        {
            var model = _generalRecycler[key].Dequeue();
            model.transform.parent = parent;
            model.SetActive(true);
            return model;
        }
        public GameObject GetFromQueue(string key, Transform parent)
        {            
            var model = _generalRecycler[key.StringToInt()].Dequeue();
            model.transform.SetParent(parent);
            model.SetActive(true);
            return model;
        }

        #endregion

        public bool DoesModelExist(string key)
        {
            return _generalRecycler.TryGetValue(key.StringToInt(), out _);
        }       
    }
}
