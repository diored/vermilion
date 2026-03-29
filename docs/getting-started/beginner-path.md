# Beginner Path

This is the shortest way to get a working bot.

Install:

- `DioRed.Vermilion`

Example:

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

This package includes:

- `Hosting`
- the Telegram connector
- the built-in chat storages
- builder-level `Use...()` shortcuts
