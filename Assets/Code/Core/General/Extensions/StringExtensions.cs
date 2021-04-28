namespace AssemblyCSharp.Assets.Code.Core.General.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveQuotes(this string value)
        {
            return value.Replace("\"", "");
        }

        public static string RemoveLeadingZero(this string value)
        {
            if (value.StartsWith("0"))
            {
                return value.Substring(1, value.Length - 1);
            }

            return value;
        }

        public static string RemoveCloneSuffix(this string value)
        {
            if (value == null)
            {
                return null;
            }

            return value.Replace("(Clone)", "").Trim();
        }
    }
}
