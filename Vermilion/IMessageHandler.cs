namespace DioRed.Vermilion;

public interface IMessageHandler
{
    Task HandleAsync(string message);
}