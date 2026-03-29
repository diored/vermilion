using DioRed.Vermilion;
using DioRed.Vermilion.ChatStorage;
using DioRed.Vermilion.Connectors;
using DioRed.Vermilion.Connectors.Telegram;
using DioRed.Vermilion.Handling;

using Microsoft.Extensions.Logging;

string botToken = Environment.GetEnvironmentVariable("VERMILION_TELEGRAM_BOT_TOKEN")
    ?? throw new InvalidOperationException(
        "Set VERMILION_TELEGRAM_BOT_TOKEN before starting the example."
    );

using ILoggerFactory loggerFactory = LoggerFactory.Create(logging =>
{
    logging
        .AddSimpleConsole(options => options.SingleLine = true)
        .SetMinimumLevel(LogLevel.Information);
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

Console.WriteLine("Bot is running. Press Ctrl+C to stop.");
await RunBotAsync(bot, cts.Token);

static async Task RunBotAsync(BotCore bot, CancellationToken ct)
{
    await bot.StartAsync(ct);

    try
    {
        await Task.Delay(Timeout.InfiniteTimeSpan, ct);
    }
    catch (OperationCanceledException) when (ct.IsCancellationRequested)
    {
        // Normal shutdown path for the sample.
    }
    finally
    {
        if (bot.State is BotCoreState.Started)
        {
            await bot.StopAsync(CancellationToken.None);
        }
    }
}
