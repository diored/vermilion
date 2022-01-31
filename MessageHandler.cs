using System.Reflection;
using System.Text.RegularExpressions;

using DioRed.Vermilion.Attributes;

namespace DioRed.Vermilion;

abstract public class MessageHandler
{
    private readonly ICollection<BotCommand> _commands;

    protected MessageHandler(MessageContext messageContext)
    {
        MessageContext = messageContext;
        ChatWriter = new ChatWriter(messageContext.BotClient, messageContext.ChatClient.Chat.Id);

        _commands = new List<BotCommand>(
            GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Select(method => (attr: method.GetCustomAttribute<BotCommandAttribute>(), method))
            .Where(x => x.attr is not null)
            .Select(x => new BotCommand
            {
                Regex = new Regex(x.attr!.Pattern),
                AdminOnly = x.method.GetCustomAttribute<AdminOnlyAttribute>() is not null,
                Handler = new Func<Task>(() =>
                {
                    return x.method.Invoke(this, null) switch
                    {
                        null => Task.CompletedTask,
                        Task task => task,
                        var x => Task.FromResult(x)
                    };
                })
            })
        );
    }

    public MessageContext MessageContext { get; }
    public IChatWriter ChatWriter { get; }

    public async virtual Task HandleAsync(string message)
    {
        var command = _commands
            .Where(h => h.Regex.IsMatch(message))
            .Take(1)
            .ToList();

        if (command.Any())
        {
            await command.First().Handler();
        }
    }
}
