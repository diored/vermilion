# Chat Storage

Vermilion expects exactly one chat storage at runtime.

Built-in storages:

- InMemory
- JsonFile
- Sqlite
- SqlServer
- AzureTable
- MongoDb

If you use separate packages with `DioRed.Vermilion.Hosting`, the usual configuration shape is:

```csharp
v.ConfigureChatStorage(s => s.UseJsonFile("vermilion-chats.json"));
```

If you use the complete package, the equivalent can be:

```csharp
v.UseJsonFileChatStorage("vermilion-chats.json");
```

Storage implementations in v15 use:

- `CancellationToken ct = default`
- streamed `GetChatsAsync(...)`
- immutable `ChatMetadata`
- typed exceptions for common storage errors

For one-off storage moves, Vermilion also includes a migration utility in [Tools/ChatStorageMigrator](/D:/bots/vermilion/Tools/ChatStorageMigrator).
It requires the target storage to be empty before migration starts.
