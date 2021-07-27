using UnityEngine;

namespace Code.Core.DataManager.GameObjects.UseCases
{
    public interface IGetTransformedGameObjectUseCase
    {
        GameObject Execute(string key, Vector3 position, Quaternion rotation);
    }
    
    public class GetTransformedGameObjectUseCase : IGetTransformedGameObjectUseCase
    {
        private readonly IDataManager _dataManager;

        public GetTransformedGameObjectUseCase(
            IDataManager dataManager)
        {
            _dataManager = dataManager;
        }
        
        public GameObject Execute(string key, Vector3 position, Quaternion rotation)
        {
            var model = _dataManager.GetGameObject(key);

            model.transform.SetPositionAndRotation(position, rotation);
            model.SetActive(true);

            return model;
        }
    }
}