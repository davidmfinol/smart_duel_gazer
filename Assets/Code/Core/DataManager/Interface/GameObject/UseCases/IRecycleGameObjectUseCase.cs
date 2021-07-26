namespace Code.Core.DataManager.Interface.GameObject.UseCases
{
    public interface IRecycleGameObjectUseCase
    {
        void Execute(string key, UnityEngine.GameObject model);
    }
}