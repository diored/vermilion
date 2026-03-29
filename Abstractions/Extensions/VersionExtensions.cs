namespace DioRed.Vermilion.Extensions;

/// <summary>
/// Version helper extensions used by Vermilion.
/// </summary>
public static class VersionExtensions
{
    extension(Version? version)
    {
        /// <summary>
        /// Normalizes a Version object to a string, removing trailing ".0" segments.
        /// </summary>
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
