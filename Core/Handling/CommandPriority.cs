namespace DioRed.Vermilion.Handling;

public enum CommandPriority : byte
{
    Lowest = 0,
    Low = 63,
    Medium = 127,
    High = 191,
    Highest = 255
}