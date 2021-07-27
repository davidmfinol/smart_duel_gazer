namespace Code.Core.General.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveLeadingZero(this string value)
        {
            return value.StartsWith("0")
                ? value.Substring(1, value.Length - 1)
                : value;
        }

        public static string RemoveCloneSuffix(this string value)
        {
            return value?.Replace("(Clone)", "").Trim();
        }
    }
}