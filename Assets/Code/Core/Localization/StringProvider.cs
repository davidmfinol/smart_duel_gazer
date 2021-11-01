using UnityEngine.Localization.Tables;

namespace Code.Core.Localization
{
    public interface IStringProvider
    {
        string GetString(string key, params object[] args);
    }
    
    public class StringProvider : IStringProvider
    {
        private readonly StringTable _stringTable;

        public StringProvider(
            StringTable stringTable)
        {
            _stringTable = stringTable;
        }

        public string GetString(string key, params object[] args)
        {
            return string.Format(_stringTable[key].LocalizedValue, args);
        }
    }
}