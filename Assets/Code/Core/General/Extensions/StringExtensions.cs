namespace AssemblyCSharp.Assets.Code.Core.General.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveQuotes(this string value)
        {
            return value.Replace("\"", "");
        }
    }
}
