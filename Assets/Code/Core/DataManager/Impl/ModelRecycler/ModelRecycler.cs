using UnityEngine;
using System.Collections.Generic;
using AssemblyCSharp.Assets.Core.DataManager.Interface.ModelRecycler;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.ModelRecycler.Entities;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;

namespace AssemblyCSharp.Assets.Core.DataManager.Impl.ModelRecycler
{
    public class ModelRecycler : IModelRecycler
    {
        private readonly Dictionary<int, Queue<GameObject>> _generalRecycler = new Dictionary<int, Queue<GameObject>>();
<<<<<<< Updated upstream
=======
        private readonly Dictionary<string, Queue<GameObject>> _modelRecycler = new Dictionary<string, Queue<GameObject>>();
>>>>>>> Stashed changes
        private readonly Dictionary<string, Texture> _images = new Dictionary<string, Texture>();

        public void CreateRecycler()
        {
            _generalRecycler.Add((int)RecyclerKeys.DestructionParticles, new Queue<GameObject>());
            _generalRecycler.Add((int)RecyclerKeys.SetCard, new Queue<GameObject>());
        }

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

        #region UseFromQueue Overloads

        public GameObject UseFromQueue(int key, Vector3 position, Quaternion rotation)
        {
            var model = _generalRecycler[key].Dequeue();
            model.transform.SetPositionAndRotation(position, rotation);
            model.SetActive(true);
            return model;
        }
        public GameObject UseFromQueue(int key)
        {
            var model = _generalRecycler[key].Dequeue();
            model.SetActive(true);
            return model;
        }
        public GameObject UseFromQueue(string key, Transform parent)
        {            
            var model = _generalRecycler[key.StringToInt()].Dequeue();
            model.transform.SetParent(parent);
            model.SetActive(true);
            return model;
        }

        #endregion

        public void CacheImage(string key, Texture texture)
        {
            _images.Add(key, texture);
        }

        public bool CheckForCachedImage(string key)
        {
            return _images.TryGetValue(key, out _);
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

        public bool CheckForExistingModel(string key)
        {
            return _generalRecycler.TryGetValue(key.StringToInt(), out var _);
        }

        public bool CheckForPlayfield()
        {
            return _generalRecycler.TryGetValue((int)RecyclerKeys.SpeedDuelPlayfield, out var _);
        }
    }
}
