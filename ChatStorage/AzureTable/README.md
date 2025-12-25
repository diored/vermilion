# DioRed.Vermilion.ChatStorage.AzureTable

Azure Table Storage based chat storage for Vermilion.

## Configuration

```json
{
  "Vermilion": {
    "AzureTable": {
      "ConnectionString": "DefaultEndpointsProtocol=...",
      "TableName": "VermilionChats"
    }
  }
}
```

## Usage

```csharp
v.ConfigureChatStorage(s => s.UseAzureTable());
```

Or explicit settings:

```csharp
v.ConfigureChatStorage(s => s.UseAzureTable(settings, o => o.TableName = "VermilionChats"));
```
