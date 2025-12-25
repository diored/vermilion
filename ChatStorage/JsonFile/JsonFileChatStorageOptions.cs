namespace DioRed.Vermilion.ChatStorage;

public class JsonFileChatStorageOptions
{
    /// <summary>Path to the JSON file that stores chat metadata.</summary>
    public required string FilePath { get; init; }

    /// <summary>Write JSON with indentation.</summary>
    public bool WriteIndented { get; set; } = true;
}
