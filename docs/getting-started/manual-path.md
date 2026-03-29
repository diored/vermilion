# Manual Path

`DioRed.Vermilion.Hosting` is optional. If you want full control, you can construct `BotCore` directly.

Example:

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

A complete runnable sample lives in `examples/QuickStart.Telegram.InMemory.Manual`.
