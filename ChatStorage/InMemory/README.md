# DioRed.Vermilion.ChatStorage.InMemory

In-memory chat storage for Vermilion.

- Best for local development, demos and tests.
- Data is **not persisted** and will be lost on restart.

Usage:

```csharp
v.ConfigureChatStorage(s => s.UseInMemory());
```
