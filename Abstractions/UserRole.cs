namespace DioRed.Vermilion;

[Flags]
public enum UserRole
{
    Bot = 1,
    Member = 2,
    ChatAdmin = 4,
    SuperAdmin = 128
}