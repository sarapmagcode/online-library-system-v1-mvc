namespace OnlineLibrarySystem_v1.Helpers
{
    public static class StringExtensions
    {
        public static string TruncateWithEllipsis(this string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Length <= maxLength ? text : $"{text.Substring(0, maxLength)}...";
        }
    }
}
