namespace DioRed.Vermilion;

public interface IMessageHandlerBuilder
{
    IMessageHandler BuildMessageHandler(MessageContext messageContext);
}