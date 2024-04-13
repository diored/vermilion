namespace DioRed.Vermilion.Handling.Context;

public class MessageArgs(string?[] args)
{
    public int Count => args.Length;
    public string? this[int index] => index < args.Length ? args[index] : null;

    public static MessageArgs Parse(string argsString, bool treatEmptyStringsAsNulls = true)
    {
        string?[] args = string.IsNullOrWhiteSpace(argsString)
            ? []
            : [
                ..argsString.Split('|', StringSplitOptions.TrimEntries)
                    .Select(arg => treatEmptyStringsAsNulls && arg is "" ? null : arg)
              ];

        return new MessageArgs(args);
    }

    public static MessageArgs Empty { get; } = new MessageArgs([]);
}