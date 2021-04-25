namespace AssemblyCSharp.Assets.Code.Core.Storage.Interface.Texture
{
    public interface ITextureStorageProvider
    {
        public UnityEngine.Texture GetTexture(string key);
        public void SaveTexture(string key, UnityEngine.Texture image);
    }
}
