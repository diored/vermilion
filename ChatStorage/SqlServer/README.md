# DioRed.Vermilion.ChatStorage.SqlServer

SQL Server based chat storage for Vermilion.

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

Or explicit connection string:

```csharp
v.ConfigureChatStorage(s => s.UseSqlServer("<CONN_STRING>", o =>
{
    o.TableName = "VermilionChats";
    o.Schema = "dbo";
}));
```
