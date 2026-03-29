# Telegram Connector

Telegram is the built-in connector currently shipped with Vermilion.

Configuration:

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

Detailed-path usage:

```csharp
v.ConfigureConnectors(c => c.AddTelegram());
```

Complete-package usage:

```csharp
v.UseTelegram();
```

Multiple accounts are also supported through configuration.
