namespace DioRed.Vermilion.Extensions;

/// <summary>
/// String helper extensions used by Vermilion.
/// </summary>
public static class StringExtensions
{
    extension(string text)
    {
        /// <summary>
        /// Splits a string by the specified separator, removing empty entries and trimming whitespace.
        /// </summary>
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
