using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using Code.Core.DataManager.Interface.GameObject.UseCases;
using UnityEngine;

namespace Code.Core.DataManager.Impl.GameObject.UseCases
{
    public class GetTransformedGameObjectUseCase : IGetTransformedGameObjectUseCase
    {
        private readonly IDataManager _dataManager;

        public GetTransformedGameObjectUseCase(
            IDataManager dataManager)
        {
            _dataManager = dataManager;
        }
        
        public UnityEngine.GameObject Execute(string key, Vector3 position, Quaternion rotation)
        {
            var model = _dataManager.GetGameObject(key);

            model.transform.SetPositionAndRotation(position, rotation);
            model.SetActive(true);

            return model;
        }
    }
}