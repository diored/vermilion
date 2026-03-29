# DioRed.Vermilion (Complete)

A convenience package that brings together Vermilion Hosting, the Telegram connector, and the built-in chat storages.

Included chat storages:
- `InMemory`
- `JsonFile`
- `Sqlite`
- `SqlServer`
- `AzureTable`
- `MongoDb`

Recommended for:
- quick start / learning
- small bots

With this package you can use the high-level builder shortcuts:

```csharp
Host.CreateDefaultBuilder(args)
    .AddVermilion("MyBot", v => v
        .UseJsonFileChatStorage("vermilion-chats.json")
        .UseTelegram()
        .ConfigureCommandHandlers(h => h.Add("/ping", () => "pong")))
    .Build()
    .Run();
```

If you want a minimal dependency graph, reference only:
- `DioRed.Vermilion.Hosting`
- one connector package
- one chat storage package

In that mode, use the collection-based API instead:

```csharp
v.ConfigureChatStorage(s => s.UseJsonFile("vermilion-chats.json"));
v.ConfigureConnectors(c => c.AddTelegram());
```

To implement your own connector/storage, reference `DioRed.Vermilion.Abstractions`.
