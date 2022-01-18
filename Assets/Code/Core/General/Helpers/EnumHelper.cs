using System;
using System.Collections.Generic;

namespace Code.Core.General.Helpers
{
    public static class EnumHelper
    {
        public static List<T> GetEnumValues<T>()
        {
            var array = (T[])Enum.GetValues(typeof(T));
            return new List<T>(array);
        }
    }
}