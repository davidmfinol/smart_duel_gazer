namespace AssemblyCSharp.Assets.Code.Core.Storage.Impl.Providers.PlayerPrefs.Interface
{
    public interface IPlayerPrefsProvider
    {
        bool HasKey(string key);
        string GetString(string key);
        void SetString(string key, string value);
    }
}
