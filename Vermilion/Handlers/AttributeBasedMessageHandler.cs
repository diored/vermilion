﻿using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace DioRed.Vermilion.Handlers;

public class AttributeBasedMessageHandler : MessageHandlerBase
{
    private static readonly Dictionary<Type, ICollection<BotCommand>> _commandCache = [];

    private readonly ICollection<BotCommand>? _commands;

    protected AttributeBasedMessageHandler(MessageContext messageContext)
        : base(messageContext)
    {
        if (messageContext.Bot.Manager.UseCommandsCache)
        {
            if (_commandCache.TryGetValue(GetType(), out var commands))
            {
                _commands = commands;
            }
            else
            {
                List<BotCommand> botCommands = LoadCommandsFromCurrentClass();
                _commands = botCommands;

                _commandCache.Add(GetType(), _commands);
            }
        }
        else
        {
            _commands = null;
        }
    }

    protected override async Task HandleMessageAsync(string message)
    {
        var commands = _commands ?? LoadCommandsFromCurrentClass();

        var command = commands
            .Where(cmd => MessageContext.UserRole.HasFlag(cmd.Role))
            .Select(cmd => (cmd, match: cmd.Regex.Match(message)))
            .Where(x => x.match.Success)
            .Take(1)
            .ToList();

        if (command.Count != 0)
        {
            (BotCommand cmd, Match match) = command.First();
            var args = match.Groups.Count > 1
                ? match.Groups.AsEnumerable<Group>().Skip(1).Select(g => g.Value).ToArray()
                : null;

            await cmd.Handler(args);
        }
    }

    private List<BotCommand> LoadCommandsFromCurrentClass()
    {
        return GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .SelectMany(method => method.GetCustomAttributes<BotCommandAttribute>().Select(attr => (attr, method)))
            .Where(x => x.attr is not null)
            .Select(x => CreateCommand(x.attr, x.method))
            .ToList();
    }

    private BotCommand CreateCommand(BotCommandAttribute attr, MethodInfo method)
    {
        RegexOptions regexOptions = RegexOptions.Multiline;

        if (attr.Options.HasFlag(BotCommandOptions.CaseInsensitive))
        {
            regexOptions |= RegexOptions.IgnoreCase;
        }

        var options = attr.Options & ~BotCommandOptions.CaseInsensitive;

        StringBuilder builder = new();

        if (options.HasFlag(BotCommandOptions.Regex))
        {
            builder.Append(attr.Command);
        }
        else
        {
            builder.Append('^');
            builder.Append(Regex.Escape(attr.Command));

            var parameters = method.GetParameters();
            if (parameters.Length > 0)
            {
                builder.Append(@"\s+");
                builder.AppendJoin(@"\s*\|\s*", parameters.Select(p => $"({GetTemplate(p)})?"));
            }

            builder.Append('$');
        }

        Regex regex = new(builder.ToString(), regexOptions);

        return new BotCommand
        {
            Regex = regex,
            Role = attr.UserRole,
            Handler = new Func<object[]?, Task>(args =>
            {
                return method.Invoke(this, args) switch
                {
                    null => Task.CompletedTask,
                    Task task => task,
                    var x => Task.FromResult(x)
                };
            })
        };
    }

    private static string GetTemplate(ParameterInfo parameterInfo)
    {
        var templateAttribute = parameterInfo.GetCustomAttribute<TemplateAttribute>();
        if (templateAttribute != null)
        {
            return templateAttribute.Pattern;
        }

        //if (parameterInfo.ParameterType == typeof(DateTime))
        //{
        //    return @"\d{4}-\d{1,2}-\d{1,2}(?: \d{1,2}:\d{2})?";
        //}

        //if (parameterInfo.ParameterType == typeof(TimeOnly))
        //{
        //    return @"\d{1,2}:\d{2}";
        //}

        //if (parameterInfo.ParameterType == typeof(int) || parameterInfo.ParameterType == typeof(long))
        //{
        //    return @"-?\d+";
        //}

        return @".+";
    }
}