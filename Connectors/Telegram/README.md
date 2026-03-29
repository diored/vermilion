# DioRed.Vermilion.Connectors.Telegram

Telegram connector for Vermilion.

## Configuration

Single account:

```json
{
  "Vermilion": {
    "Telegram": {
      "BotToken": "<TOKEN>",
      "SuperAdmins": "123,456"
    }
  }
}
```

Multiple accounts:

```json
{
  "Vermilion": {
    "Telegram": {
      "Accounts": "main,alt",
      "main": { "BotToken": "<TOKEN1>" },
      "alt":  { "BotToken": "<TOKEN2>" }
    }
  }
}
```

## Usage

```csharp
v.ConfigureConnectors(c => c.AddTelegram());
```

If you use the complete `DioRed.Vermilion` package, you can also write:

```csharp
v.UseTelegram();
```

Or explicit token:

```csharp
v.ConfigureConnectors(c => c.AddTelegram("<TOKEN>"));
```
