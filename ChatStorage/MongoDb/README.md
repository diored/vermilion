# DioRed.Vermilion.ChatStorage.MongoDb

MongoDB based chat storage for Vermilion.

The package applies idempotent storage migrations automatically on startup. Upgrading the NuGet package updates indexes and normalizes legacy documents in place when needed.

## Configuration

```json
{
  "Vermilion": {
    "MongoDb": {
      "ConnectionString": "mongodb://localhost:27017",
      "DatabaseName": "Vermilion",
      "CollectionName": "Chats"
    }
  }
}
```

## Usage

```csharp
v.ConfigureChatStorage(s => s.UseMongoDb());
```

Or explicit connection string:

```csharp
v.ConfigureChatStorage(s => s.UseMongoDb("mongodb://localhost:27017", o =>
{
    o.DatabaseName = "Vermilion";
    o.CollectionName = "Chats";
}));
```
