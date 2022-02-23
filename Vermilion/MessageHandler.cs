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
            .SelectMany(method => method.GetCustomAttributes<BotCommandAttribute>().Select(attr => (attr, method)))
            .Where(x => x.attr is not null)
            .Select(x => new BotCommand
            {
                Regex = x.attr.Regex,
                Role = x.attr.Role,
                Handler = new Func<string[]?, Task>(args =>
                {
                    return x.method.Invoke(this, args) switch
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
            .Where(cmd => MessageContext.Role.HasFlag(cmd.Role))
            .Select(cmd => (cmd, match: cmd.Regex.Match(message)))
            .Where(x => x.match.Success)
            .Take(1)
            .ToList();

        if (command.Any())
        {
            (BotCommand cmd, Match match) = command.First();
            var args = match.Groups.Count > 1
                ? match.Groups.AsEnumerable<Group>().Skip(1).Select(g => g.Value).ToArray()
                : null;

            try
            {
                await cmd.Handler(args);
            }
            catch (Exception ex)
            {
                await OnExceptionAsync(ex);
            }
        }
    }

    public virtual async Task OnExceptionAsync(Exception ex)
    {
        await ChatWriter.SendTextAsync($"Error occured: {ex.Message}");
    }
}
