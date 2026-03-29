using Microsoft.Extensions.Logging;

namespace DioRed.Vermilion;

/// <summary>
/// Contains logging event identifiers used by Vermilion components.
/// </summary>
public static class Events
{
    // 1xxx - Vermilion events

    // 11xx — message handling
    // 119x - error
    /// <summary>
    /// Logged after a message has been handled successfully.
    /// </summary>
    public static EventId MessageHandled { get; } = new EventId(1100, "Message handled");

    /// <summary>
    /// Logged when message handling ends with an exception.
    /// </summary>
    public static EventId MessageHandleException { get; } = new EventId(1190, "Message handler exception");

    // 12xx - chat management
    // 121x - chat adding
    /// <summary>
    /// Logged after a chat is added to storage.
    /// </summary>
    public static EventId ChatAdded { get; } = new EventId(1210, "Chat added");

    /// <summary>
    /// Logged when adding a chat fails.
    /// </summary>
    public static EventId ChatAddFailure { get; } = new EventId(1219, "Chat add failure");

    // 122x - chat removing
    /// <summary>
    /// Logged after a chat is removed from storage.
    /// </summary>
    public static EventId ChatRemoved { get; } = new EventId(1220, "Chat removed");

    /// <summary>
    /// Logged when removing a chat fails.
    /// </summary>
    public static EventId ChatRemoveFailure { get; } = new EventId(1229, "Chat remove failure");

    // 13xx - jobs
    // 131x - job starting
    /// <summary>
    /// Logged when a scheduled job starts running.
    /// </summary>
    public static EventId JobStarted { get; } = new EventId(1310, "Job started");

    /// <summary>
    /// Logged when starting a scheduled job fails.
    /// </summary>
    public static EventId JobStartFailure { get; } = new EventId(1319, "Job start failure"); // not in use

    // 132x - job finishing
    /// <summary>
    /// Logged when a scheduled job finishes one occurrence.
    /// </summary>
    public static EventId JobFinished { get; } = new EventId(1320, "Job finished");

    /// <summary>
    /// Logged when finishing a scheduled job occurrence fails.
    /// </summary>
    public static EventId JobFinishFailure { get; } = new EventId(1329, "Job finish failure"); // not in use

    // 133x - job scheduling
    /// <summary>
    /// Logged when the next job occurrence is scheduled.
    /// </summary>
    public static EventId JobScheduled { get; } = new EventId(1330, "Job scheduled");

    /// <summary>
    /// Logged when job scheduling fails.
    /// </summary>
    public static EventId JobScheduleFailure { get; } = new EventId(1339, "Job schedule failure"); // not in use

    // 134x - job execution
    /// <summary>
    /// Logged when a scheduled job completes permanently.
    /// </summary>
    public static EventId JobCompleted { get; } = new EventId(1340, "Job completed");

    /// <summary>
    /// Logged when a scheduled job is cancelled.
    /// </summary>
    public static EventId JobCancelled { get; } = new EventId(1341, "Job cancelled");

    /// <summary>
    /// Logged when a scheduled job faults.
    /// </summary>
    public static EventId JobFailed { get; } = new EventId(1349, "Job failed");
}
