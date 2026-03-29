# DioRed.Vermilion.ChatStorage.SqlServer

SQL Server based chat storage for Vermilion.

The package applies idempotent schema migrations automatically on startup. Upgrading the NuGet package upgrades the storage schema in place when needed.

## Configuration

```json
{
  "Vermilion": {
    "SqlServer": {
      "ConnectionString": "Server=...;Database=...;Trusted_Connection=True;",
      "Schema": "dbo",
      "TableName": "VermilionChats"
    }
  }
}
```

## Usage

```csharp
v.ConfigureChatStorage(s => s.UseSqlServer());
```

If you use the complete `DioRed.Vermilion` package, you can also write:

```csharp
v.UseSqlServerChatStorage();
```

Or explicit connection string:

```csharp
v.ConfigureChatStorage(s => s.UseSqlServer("<CONN_STRING>", o =>
{
    o.TableName = "VermilionChats";
    o.Schema = "dbo";
}));
```
