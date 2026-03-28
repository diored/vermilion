namespace DioRed.Vermilion.ChatStorage;

public class StorageMigrationException : Exception
{
    public StorageMigrationException(string message)
        : base(message)
    {
    }

    public StorageMigrationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
