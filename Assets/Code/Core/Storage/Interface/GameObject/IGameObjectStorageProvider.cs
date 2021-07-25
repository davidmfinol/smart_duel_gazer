namespace AssemblyCSharp.Assets.Code.Core.Storage.Interface.GameObject
{
    public interface IGameObjectStorageProvider
    {
        public UnityEngine.GameObject GetGameObject(string key);
        public void SaveGameObject(string key, UnityEngine.GameObject model);
        public void RemoveGameObject(string key);

        public UnityEngine.GameObject GetCardModel(int cardId);
    }
}
