using AssemblyCSharp.Assets.Code.Core.Storage.Impl.Providers.PlayerPrefs.Interface;

namespace AssemblyCSharp.Assets.Code.Core.Storage.Impl.Providers.PlayerPrefs.Impl
{
    public class PlayerPrefsProvider : IPlayerPrefsProvider
    {
        public bool HasKey(string key)
        {
            return UnityEngine.PlayerPrefs.HasKey(key);
        }

        public string GetString(string key)
        {
            return UnityEngine.PlayerPrefs.GetString(key);
        }

        public void SetString(string key, string value)
        {
            UnityEngine.PlayerPrefs.SetString(key, value);
        }
    }
}
