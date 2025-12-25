namespace DioRed.Vermilion.Extensions;

public static class VersionExtensions
{
    extension(Version? version)
    {
        /// <summary>
        /// Normalizes a Version object to a string, removing trailing ".0" segments.
        /// </summary>
        /// <param name="version">The version to normalize.</param>
        /// <returns>A normalized version string.</returns>
        public string Normalize()
        {
            return version?.ToString() switch
            {
                null => "0.0",
                var v when v.EndsWith(".0.0") => v[..^4],
                var v when v.EndsWith(".0") => v[..^2],
                var v => v
            };
        }
    }
}