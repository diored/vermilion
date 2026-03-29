# Detailed Path

Use this path if you want a smaller dependency graph and more explicit package choices.

Reference:

- `DioRed.Vermilion.Hosting`
- one connector package
- one chat storage package

Example:

```csharp
using DioRed.Vermilion.Hosting;
using Microsoft.Extensions.Hosting;

Host.CreateDefaultBuilder(args)
    .AddVermilion("MyBot", v =>
    {
        v.ConfigureChatStorage(s => s.UseJsonFile("vermilion-chats.json"));
        v.ConfigureConnectors(c => c.AddTelegram());
        v.ConfigureCommandHandlers(h =>
            h.Add("/ping", () => "pong"));
    })
    .Build()
    .Run();
```

This mode uses collection-based configuration rather than the high-level `Use...()` builder shortcuts from the complete package.

Typical extensions in this mode:

```csharp
v.ConfigureScheduledJobs(j => j.LoadFromEntryAssembly());
v.ConfigureClientsPolicy(p => p.AllowForEveryone());
```
