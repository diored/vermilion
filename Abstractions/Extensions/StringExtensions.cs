namespace DioRed.Vermilion.Extensions;

public static class StringExtensions
{
    extension(string text)
    {
        /// <summary>
        /// Splits a string by the specified separator, removing empty entries and trimming whitespace.
        /// </summary>
        /// <param name="text">The string to split.</param>
        /// <param name="separator">The character to split by.</param>
        /// <returns>An array of split and trimmed strings.</returns>
        public string[] SplitBy(char separator)
        {
            return text.Split(
                separator,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            );
        }
    }
}