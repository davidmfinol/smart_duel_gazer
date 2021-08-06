using UnityEngine;

namespace Code.Wrappers.WrapperPlayerPrefs
{
    public interface IPlayerPrefsProvider
    {
        bool HasKey(string key);
        string GetString(string key);
        void SetString(string key, string value);
        bool GetBool(string key);
        void SetBool(string key, bool state);
    }

    public class PlayerPrefsProvider : IPlayerPrefsProvider
    {
        public bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public string GetString(string key)
        {
            return PlayerPrefs.GetString(key);
        }

        public void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }

        public bool GetBool(string key)
        {
            return PlayerPrefs.GetInt(key) == 1;
        }

        public void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }
    }
}