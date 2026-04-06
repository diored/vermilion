# Chat Storage Migrator

One-off utility for copying Vermilion chat data from one storage provider to another.

The target storage must be empty before migration starts. This avoids conflict-resolution logic and keeps the migration deterministic.

## Install

```powershell
dotnet tool install --global DioRed.Vermilion.Tools
```

## Run

```powershell
vermilion-tools --config Tools\ChatStorageMigrator\appsettings.sample.json
```

For local repository development, you can still run:

```powershell
dotnet run --project Tools\ChatStorageMigrator\Vermilion.Tools.csproj -- --config Tools\ChatStorageMigrator\appsettings.sample.json
```

Replace the sample settings with real source and target connection details first.

## Configuration

```json
{
  "VermilionMigration": {
    "Source": {
      "Provider": "AzureTable",
      "AzureTable": {
        "ConnectionString": "DefaultEndpointsProtocol=https;...",
        "TableName": "VermilionChats"
      }
    },
    "Target": {
      "Provider": "MongoDb",
      "MongoDb": {
        "ConnectionString": "mongodb://localhost:27017",
        "DatabaseName": "Vermilion",
        "CollectionName": "Chats"
      }
    }
  }
}
```

Supported providers:

- `AzureTable`
- `MongoDb`
- `Sqlite`
- `SqlServer`
- `JsonFile`
- `InMemory`

Titles are preserved when the source storage supports export of persisted titles.
