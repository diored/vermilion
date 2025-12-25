# Vermilion

Vermilion is a small .NET chat-bot engine focused on **simple setup** and **swappable implementations**:

- exactly one **ChatStorage** (required)
- one or more **Connectors** (required)
- a set of **CommandHandlers** (required)

## Quick start (Telegram + JSON file storage)

Install the *complete* package (recommended for beginners):

- `DioRed.Vermilion`

Then configure Vermilion in a Generic Host:

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

`appsettings.json`:

```json
{
  "Vermilion": {
    "Telegram": {
      "BotToken": "<PUT_YOUR_BOT_TOKEN_HERE>"
    }
  }
}
```

## Advanced

If you want to implement your own connector or chat storage, reference `DioRed.Vermilion.Abstractions`.

## Examples
See the `examples/` folder.
