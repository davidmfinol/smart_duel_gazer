namespace AssemblyCSharp.Assets.Code.Core.DataManager.Interface.GameObject
{
    public interface IGameObjectDataManager
    {
        public UnityEngine.GameObject GetGameObject(string key);
        public void SaveGameObject(string key, UnityEngine.GameObject model);
        public void RemoveGameObject(string key);

        UnityEngine.GameObject GetCardModel(string cardId);
    }
}
