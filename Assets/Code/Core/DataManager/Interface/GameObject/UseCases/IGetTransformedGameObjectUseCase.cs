using UnityEngine;

namespace Code.Core.DataManager.Interface.GameObject.UseCases
{
    public interface IGetTransformedGameObjectUseCase
    {
        UnityEngine.GameObject Execute(string key, Vector3 position, Quaternion rotation);
    }
}