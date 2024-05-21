using Microsoft.Extensions.Logging;

namespace DioRed.Vermilion;

public static class Events
{
    // 1xxx - Vermilion events

    // 11xx — message handling
    // 119x - error
    public static EventId MessageHandled { get; } = new EventId(1100, "Message handled");
    public static EventId MessageHandleException { get; } = new EventId(1190, "Message handler exception");

    // 12xx - chat management
    // 121x - chat adding
    public static EventId ChatAdded { get; } = new EventId(1210, "Chat added");
    public static EventId ChatAddFailure { get; } = new EventId(1219, "Chat add failure");

    // 122x - chat removing
    public static EventId ChatRemoved { get; } = new EventId(1220, "Chat removed");
    public static EventId ChatRemoveFailure { get; } = new EventId(1229, "Chat remove failure");

    // 13xx - jobs
    // 131x - job starting
    public static EventId JobStarted { get; } = new EventId(1310, "Job started");
    public static EventId JobStartFailure { get; } = new EventId(1319, "Job start failure"); // not in use

    // 132x - job finishing
    public static EventId JobFinished { get; } = new EventId(1320, "Job finished");
    public static EventId JobFinishFailure { get; } = new EventId(1329, "Job finish failure"); // not in use

    // 133x - job scheduling
    public static EventId JobScheduled { get; } = new EventId(1330, "Job scheduled");
    public static EventId JobScheduleFailure { get; } = new EventId(1339, "Job schedule failure"); // not in use
}