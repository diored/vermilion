# DioRed.Vermilion.ChatStorage.Sqlite

SQLite based chat storage for Vermilion.

The package applies idempotent schema migrations automatically on startup. Upgrading the NuGet package updates the local database schema in place when needed.

## Configuration

```json
{
  "Vermilion": {
    "Sqlite": {
      "ConnectionString": "Data Source=vermilion.db",
      "TableName": "Chats"
    }
  }
}
```

## Usage

```csharp
v.ConfigureChatStorage(s => s.UseSqlite());
```

Or explicit connection string:

```csharp
v.ConfigureChatStorage(s => s.UseSqlite("Data Source=vermilion.db", o =>
{
    o.TableName = "Chats";
}));
```
