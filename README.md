# Vermilion

Vermilion is a small .NET chat-bot engine focused on **simple setup** and **swappable implementations**:

- exactly one **ChatStorage** (required)
- one or more **Connectors** (required)
- a set of **CommandHandlers** (required)

Choose the path that fits your goal:

- Beginner path:
  Install `DioRed.Vermilion` and use the shortest startup syntax.
- Detailed path:
  Reference `DioRed.Vermilion.Hosting` plus separate connector/storage packages.
- Manual path:
  Construct `BotCore` directly without `Hosting`.

## Beginner Path

Install the complete package:

- `DioRed.Vermilion`

Then configure Vermilion in a Generic Host:

```csharp
using DioRed.Vermilion.Hosting;
using Microsoft.Extensions.Hosting;

Host.CreateDefaultBuilder(args)
    .AddVermilion("MyBot", v => v
        .UseJsonFileChatStorage("vermilion-chats.json")
        .UseTelegram()
        .ConfigureCommandHandlers(h =>
            h.Add("/ping", () => "pong")))
    .Build()
    .Run();
```

This is the shortest path. The complete package includes:

- `Hosting`
- the Telegram connector
- the built-in chat storages
- the `Use...()` builder shortcuts

Minimal `appsettings.json`:

```json
{
  "Vermilion": {
    "Telegram": {
      "BotToken": "<PUT_YOUR_BOT_TOKEN_HERE>"
    }
  }
}
```

## Detailed Path

If you want a smaller dependency graph, reference:

- `DioRed.Vermilion.Hosting`
- one connector package
- one chat storage package

In that mode, use the collection-based API:

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

If you need more framework features, this is also the path where you typically add:

```csharp
v.ConfigureScheduledJobs(j => j.LoadFromEntryAssembly());
v.Public();
```

## Manual Path

`DioRed.Vermilion.Hosting` is optional. If you want full control, you can build `BotCore` directly:

```csharp
using DioRed.Vermilion;
using DioRed.Vermilion.ChatStorage;
using DioRed.Vermilion.Connectors;
using DioRed.Vermilion.Connectors.Telegram;
using DioRed.Vermilion.Handling;
using Microsoft.Extensions.Logging;

string botToken = Environment.GetEnvironmentVariable("VERMILION_TELEGRAM_BOT_TOKEN")
    ?? throw new InvalidOperationException("Set VERMILION_TELEGRAM_BOT_TOKEN.");

using ILoggerFactory loggerFactory = LoggerFactory.Create(logging =>
{
    logging.AddSimpleConsole();
});

BotCore bot = new(
    new BotCoreSettings
    {
        ChatStorage = new InMemoryChatStorage(),
        Connectors =
        [
            new KeyValuePair<string, IConnector>(
                Defaults.ConnectorKey,
                new TelegramConnector(
                    new TelegramConnectorOptions
                    {
                        BotToken = botToken,
                        ConnectorKey = Defaults.ConnectorKey
                    },
                    loggerFactory
                )
            )
        ],
        CommandHandlers =
        [
            new SimpleCommandHandler(
                new CommandDefinition
                {
                    Template = "/ping"
                },
                (context, feedback, ct) => feedback.TextAsync("pong", ct)
            )
        ]
    },
    loggerFactory.CreateLogger<BotCore>()
);

using CancellationTokenSource cts = new();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

await bot.RunAsync(cts.Token);
```

A complete runnable sample lives in [examples/QuickStart.Telegram.InMemory.Manual](/D:/bots/vermilion/examples/QuickStart.Telegram.InMemory.Manual).

## Advanced

If you want to implement your own connector or chat storage, reference `DioRed.Vermilion.Abstractions`.

## Tools

If you need to move chats from one storage provider to another, use the one-off migration utility in [Tools/ChatStorageMigrator](/D:/bots/vermilion/Tools/ChatStorageMigrator).
It copies chats from source to target only when the target storage is empty, so migration stays deterministic and does not need conflict resolution.
The packaged CLI is published as `DioRed.Vermilion.Tools`.

## Upgrade notes

If you are upgrading from `v14` to `v15`, see [MIGRATION.md](./MIGRATION.md).

## Examples
See the `examples/` folder.
