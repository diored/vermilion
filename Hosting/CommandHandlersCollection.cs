using System.Reflection;

using DioRed.Vermilion.Handling;

using DioRed.Vermilion.Messages;

namespace DioRed.Vermilion.Hosting;
public class CommandHandlersCollection(IServiceProvider services)
{
    internal List<ICommandHandler> CommandHandlers { get; } = [];

    public CommandHandlersCollection Add(
        string command,
        Func<string, string> replyFunc,
        UserRole requiredRole = UserRole.Member
    )
    {
        return Add(
            new SimpleCommandHandler(
                new CommandDefinition
                {
                    Template = command,
                    TailPolicy = TailPolicy.HasTail,
                    RequiredRole = requiredRole
                },
                (context, send) => send.TextAsync(replyFunc(context.Message.Tail))
            )
        );
    }

    public CommandHandlersCollection Add(
        string command,
        Func<string> replyFunc,
        UserRole requiredRole = UserRole.Member
    )
    {
        return Add(
            new SimpleCommandHandler(
                new CommandDefinition
                {
                    Template = command,
                    TailPolicy = TailPolicy.HasNoTail,
                    RequiredRole = requiredRole
                },
                (context, send) => send.TextAsync(replyFunc())
            )
        );
    }

    public CommandHandlersCollection Add(ICommandHandler commandHandler)
    {
        CommandHandlers.Add(commandHandler);

        return this;
    }

    public CommandHandlersCollection Add(Type type)
    {
        if (!typeof(ICommandHandler).IsAssignableFrom(type))
        {
            throw new ArgumentException(
                string.Format(
                    ExceptionMessages.TypeDoesntImplementTheInterface_2,
                    type.Name,
                    nameof(ICommandHandler)
                ),
                nameof(type)
            );
        }

        return Add((ICommandHandler)TypeResolver.Resolve(services, type));
    }

    public CommandHandlersCollection Add<T>()
        where T : ICommandHandler
    {
        return Add(TypeResolver.Resolve<T>(services));
    }

    public CommandHandlersCollection LoadFromEntryAssembly()
    {
        return LoadFromAssembly(Assembly.GetEntryAssembly()!);
    }

    public CommandHandlersCollection LoadFromAssembly(Assembly assembly)
    {
        TypeInfo[] commandHandlers =
        [
            ..assembly.DefinedTypes
                .Where(typeInfo =>
                    typeInfo.IsClass &&
                    !typeInfo.IsAbstract &&
                    typeInfo.ImplementedInterfaces.Contains(typeof(ICommandHandler)))
        ];

        foreach (TypeInfo typeInfo in commandHandlers)
        {
            Add(typeInfo.AsType());
        }

        return this;
    }

    public CommandHandlersCollection LoadFromAssemblies(IEnumerable<Assembly> assemblies)
    {
        foreach (Assembly assembly in assemblies)
        {
            LoadFromAssembly(assembly);
        }

        return this;
    }
}