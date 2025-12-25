# DioRed.Vermilion.ChatStorage.JsonFile

A simple **single-file JSON** implementation of `IChatStorage`.

## Use it

```csharp
using DioRed.Vermilion.Hosting;

builder.AddVermilion("MyBot", v =>
{
    v.ConfigureChatStorage(s => s.UseJsonFile("./vermilion-chats.json"));
    v.ConfigureConnectors(c => c.AddTelegram());
});
```

## Configuration

```json
{
  "Vermilion": {
    "JsonFile": {
      "FilePath": "./vermilion-chats.json",
      "WriteIndented": true
    }
  }
}
```

Notes:
- writes are **atomic** (temp file + replace)
- the storage is process-local (no cross-process locking)
