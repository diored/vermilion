# Migration Guide

This document covers the upgrade from Vermilion `v14` to `v15`.

`v15` is a breaking release. The main goals are:

- make async APIs cancellation-aware
- make chat storage contracts more explicit
- remove mutable runtime chat metadata
- make storage providers safer and more scalable

## Summary

The most important changes are:

- `IChatStorage` now uses `CancellationToken ct = default`
- `IChatStorage.GetChatsAsync(...)` now returns `IAsyncEnumerable<ChatMetadata>`
- `ChatMetadata` is now immutable
- chat titles remain storage-only and are not part of `ChatMetadata`
- `IConnector.PostAsync(...)` now accepts `CancellationToken ct = default`
- `ICommandHandler.HandleAsync(...)` now accepts `CancellationToken ct = default`
- storage implementations now throw typed exceptions instead of generic `ArgumentException` / `InvalidOperationException`

## Storage Implementers

Old `v14` shape:

```csharp
public interface IChatStorage
{
    Task AddChatAsync(ChatMetadata metadata);
    Task AddChatAsync(ChatMetadata metadata, string title);
    Task<ChatMetadata> GetChatAsync(ChatId chatId);
    Task<ChatMetadata[]> GetChatsAsync();
    Task RemoveChatAsync(ChatId chatId);
    Task UpdateChatAsync(ChatMetadata metadata);
}
```

New `v15` shape:

```csharp
public interface IChatStorage
{
    Task AddChatAsync(
        ChatMetadata metadata,
        string? title = null,
        CancellationToken ct = default
    );

    Task<ChatMetadata> GetChatAsync(ChatId chatId, CancellationToken ct = default);
    IAsyncEnumerable<ChatMetadata> GetChatsAsync(CancellationToken ct = default);
    Task RemoveChatAsync(ChatId chatId, CancellationToken ct = default);
    Task UpdateChatAsync(ChatMetadata metadata, CancellationToken ct = default);
}
```

What to change in custom storage providers:

- merge both `AddChatAsync(...)` overloads into one
- accept `CancellationToken ct = default` in every async method
- return streamed results from `GetChatsAsync(...)`
- throw:
  - `ChatAlreadyExistsException`
  - `ChatNotFoundException`
  - `StorageMigrationException` where migration fails

Example:

```csharp
public async IAsyncEnumerable<ChatMetadata> GetChatsAsync(
    [EnumeratorCancellation] CancellationToken ct = default
)
{
    foreach (ChatMetadata chat in _chats.Values)
    {
        ct.ThrowIfCancellationRequested();
        yield return chat;
    }
}
```

## Connector Implementers

Old `v14` shape:

```csharp
Task<PostResult> PostAsync(long internalId, IContent content);
```

New `v15` shape:

```csharp
Task<PostResult> PostAsync(long internalId, IContent content, CancellationToken ct = default);
```

What to change:

- accept `ct` in outbound delivery
- pass `ct` into HTTP, database, SDK, or delay/retry operations where possible

`StartAsync(...)` and `StopAsync(...)` already used cancellation tokens before `v15`, but the preferred parameter name is now `ct`.

## Command Handler Implementers

Old `v14` shape:

```csharp
Task<bool> HandleAsync(MessageHandlingContext context, Feedback feedback);
```

New `v15` shape:

```csharp
Task<bool> HandleAsync(
    MessageHandlingContext context,
    Feedback feedback,
    CancellationToken ct = default
);
```

What to change:

- add `CancellationToken ct = default`
- use `ct` in your I/O and long-running operations
- pass `ct` through to downstream services where appropriate

Example:

```csharp
public async Task<bool> HandleAsync(
    MessageHandlingContext context,
    Feedback feedback,
    CancellationToken ct = default
)
{
    string text = await service.BuildReplyAsync(context.Message.Tail, ct);
    await feedback.TextAsync(text, ct);
    return true;
}
```

## Scheduled Jobs

`v15` uses the more general `IScheduledJob` contract instead of the old daily-only abstraction.

Old `v13` / `v14` style:

```csharp
internal class GoodMorningDailyJob : IDailyJob
{
    public DailyJobDefinition Definition { get; } = new()
    {
        TimeOfDay = new TimeOnly(7, 40, 0),
        Id = "Good morning"
    };

    public async Task Handle(IServiceProvider services, BotCore botCore)
    {
        // ...
    }
}
```

Preferred `v15` style:

```csharp
internal class GoodMorningDailyJob : IScheduledJob
{
    public ScheduledJobDefinition Definition { get; } = new()
    {
        Id = "Good morning",
        Schedule = new LocalTimeDailySchedule(new TimeOnly(7, 40, 0))
    };

    public async Task HandleAsync(IServiceProvider services, BotCore botCore, CancellationToken ct)
    {
        // ...
    }
}
```

Compatibility notes:

- `IDailyJob` and `DailyJobDefinition` are available again in `v15` as obsolete compatibility APIs
- old `DailyJobDefinition.TimeOfDay` is mapped to `LocalTimeDailySchedule`
- old `IDailyJob.Handle(IServiceProvider, BotCore)` still works, but new code should implement `HandleAsync(..., CancellationToken ct)`
- for new development, prefer `IScheduledJob` directly

## ChatMetadata

`ChatMetadata` is now immutable and includes helper methods for tag updates.

Old pattern:

```csharp
chatClient.Metadata.Tags.Add("vip");
await storage.UpdateChatAsync(chatClient.Metadata);
```

Recommended `v15` pattern:

```csharp
ChatMetadata updated = chatClient.Metadata.WithTag("vip");

await storage.UpdateChatAsync(updated, ct);
```

Bulk updates:

```csharp
ChatMetadata updated = chatClient.Metadata
    .WithTags(["vip", "beta"])
    .WithoutTag("new-user");
```

Checking tags:

```csharp
if (chatClient.Metadata.HasTag("vip"))
{
    // ...
}
```

Notes:

- `ChatMetadata` represents runtime chat identity and tags only
- `Title` is intentionally not part of `ChatMetadata`
- storage providers may persist `title` for diagnostics or manual DB inspection

## Reading Chats

Old pattern:

```csharp
ChatMetadata[] chats = await storage.GetChatsAsync();
```

New pattern:

```csharp
await foreach (ChatMetadata chat in storage.GetChatsAsync(ct))
{
    // use chat
}
```

If you need a materialized collection:

```csharp
ChatMetadata[] chats = await storage.GetChatsAsync(ct).ToArrayAsync(ct);
```

## Typed Exceptions

If your code previously caught generic exceptions, update it to catch the new public exception types where appropriate.

Example:

```csharp
try
{
    await storage.AddChatAsync(metadata, title, ct);
}
catch (ChatAlreadyExistsException)
{
    // ignore or reconcile
}
```

## Hosting Helpers

`SimpleCommandHandler` and hosting registration helpers remain ergonomic:

- existing sync delegates still work
- async delegates with `CancellationToken` are now supported

Example:

```csharp
v.ConfigureCommandHandlers(h =>
{
    h.Add("/ping", () => "pong");
    h.Add("/echo", (tail, ct) => Task.FromResult(tail));
});
```

## Checklist

Before publishing or shipping your upgrade:

1. Update all custom `IChatStorage` implementations.
2. Update all custom `IConnector` implementations.
3. Update all custom `ICommandHandler` implementations.
4. Replace `GetChatsAsync()` array assumptions with async streaming.
5. Replace in-place `ChatMetadata.Tags` mutation with `WithTag(...)`, `WithoutTag(...)`, `WithTags(...)`, or `WithoutTags(...)`.
6. Update exception handling to use typed Vermilion exceptions.
7. Run your unit and integration tests against the `ProjectRefs` solution first.
