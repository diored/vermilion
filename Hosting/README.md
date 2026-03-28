# DioRed.Vermilion.Hosting

Generic Host integration for Vermilion.

Main entry point is `IHostBuilder.AddVermilion(...)`, which configures logging and registers a hosted service.

```csharp
using DioRed.Vermilion.Hosting;
using Microsoft.Extensions.Hosting;

Host.CreateDefaultBuilder(args)
    .AddVermilion("MyBot", v =>
    {
        v.ConfigureChatStorage(s => s.UseInMemory());
        v.ConfigureConnectors(c => c.AddTelegram());
        v.ConfigureCommandHandlers(h => h.Add("/ping", () => "pong"));
    })
    .Build()
    .Run();
```

> Note: Vermilion requires exactly one ChatStorage, at least one Connector, and at least one CommandHandler.

`v15` notes:

- command handlers are cancellation-aware
- simple registration helpers support both sync delegates and `CancellationToken`-aware async delegates
- chat storage providers use streaming reads and immutable `ChatMetadata`
