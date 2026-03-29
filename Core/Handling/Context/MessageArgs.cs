namespace DioRed.Vermilion.Handling.Context;

/// <summary>
/// Represents command arguments parsed from a message tail.
/// </summary>
public class MessageArgs(string?[] args)
{
    /// <summary>
    /// Gets the number of parsed arguments.
    /// </summary>
    public int Count => args.Length;

    /// <summary>
    /// Gets the argument at the specified index, or <c>null</c> if it is missing.
    /// </summary>
    public string? this[int index] => index < args.Length ? args[index] : null;

    /// <summary>
    /// Parses arguments from a pipe-separated string.
    /// </summary>
    public static MessageArgs Parse(
        string argsString,
        bool treatEmptyStringsAsNulls = true
    )
    {
        string?[] args = string.IsNullOrWhiteSpace(argsString)
            ? []
            : [
                ..argsString.Split('|', StringSplitOptions.TrimEntries)
                    .Select(arg => treatEmptyStringsAsNulls && arg is "" ? null : arg)
              ];

        return new MessageArgs(args);
    }

    /// <summary>
    /// Gets an empty argument collection.
    /// </summary>
    public static MessageArgs Empty { get; } = new MessageArgs([]);
}
