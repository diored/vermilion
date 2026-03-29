using System.Reflection;

using DioRed.Vermilion.Handling;
using DioRed.Vermilion.Handling.Context;

using DioRed.Vermilion.Messages;

namespace DioRed.Vermilion.Hosting;

/// <summary>
/// Builder-style collection used to register command handlers for Vermilion.
/// </summary>
public class CommandHandlersCollection(IServiceProvider services)
{
    internal List<ICommandHandler> CommandHandlers { get; } = [];

    /// <summary>
    /// Registers a command that requires a tail and returns a synchronous text response.
    /// </summary>
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
                (context, send, ct) => send.TextAsync(replyFunc(context.Message.Tail), ct)
            )
        );
    }

    /// <summary>
    /// Registers a command that returns a synchronous text response based on the full handling context.
    /// </summary>
    public CommandHandlersCollection Add(
        string command,
        Func<MessageHandlingContext, string> replyFunc,
        TailPolicy tailPolicy = TailPolicy.Any,
        UserRole requiredRole = UserRole.Member
    )
    {
        return Add(
            new SimpleCommandHandler(
                new CommandDefinition
                {
                    Template = command,
                    TailPolicy = tailPolicy,
                    RequiredRole = requiredRole
                },
                (context, send, ct) => send.TextAsync(replyFunc(context), ct)
            )
        );
    }

    /// <summary>
    /// Registers a command that requires a tail and returns an asynchronous text response.
    /// </summary>
    public CommandHandlersCollection Add(
        string command,
        Func<string, CancellationToken, Task<string>> replyFunc,
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
                async (context, send, ct) => await send.TextAsync(
                    await replyFunc(context.Message.Tail, ct),
                    ct
                )
            )
        );
    }

    /// <summary>
    /// Registers a command that returns an asynchronous text response based on the full handling context.
    /// </summary>
    public CommandHandlersCollection Add(
        string command,
        Func<MessageHandlingContext, CancellationToken, Task<string>> replyFunc,
        TailPolicy tailPolicy = TailPolicy.Any,
        UserRole requiredRole = UserRole.Member
    )
    {
        return Add(
            new SimpleCommandHandler(
                new CommandDefinition
                {
                    Template = command,
                    TailPolicy = tailPolicy,
                    RequiredRole = requiredRole
                },
                async (context, send, ct) => await send.TextAsync(
                    await replyFunc(context, ct),
                    ct
                )
            )
        );
    }

    /// <summary>
    /// Registers a command without a tail that returns a synchronous text response.
    /// </summary>
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
                (context, send, ct) => send.TextAsync(replyFunc(), ct)
            )
        );
    }

    /// <summary>
    /// Registers a command without a tail that returns an asynchronous text response.
    /// </summary>
    public CommandHandlersCollection Add(
        string command,
        Func<CancellationToken, Task<string>> replyFunc,
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
                async (_, send, ct) => await send.TextAsync(
                    await replyFunc(ct),
                    ct
                )
            )
        );
    }

    /// <summary>
    /// Adds an already constructed command handler instance.
    /// </summary>
    public CommandHandlersCollection Add(ICommandHandler commandHandler)
    {
        CommandHandlers.Add(commandHandler);

        return this;
    }

    /// <summary>
    /// Resolves and adds a command handler by type from the service provider.
    /// </summary>
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

    /// <summary>
    /// Resolves and adds a command handler by generic type from the service provider.
    /// </summary>
    public CommandHandlersCollection Add<T>()
        where T : ICommandHandler
    {
        return Add(TypeResolver.Resolve<T>(services));
    }

    /// <summary>
    /// Loads all command handlers from the entry assembly.
    /// </summary>
    public CommandHandlersCollection LoadFromEntryAssembly()
    {
        return LoadFromAssembly(Assembly.GetEntryAssembly()!);
    }

    /// <summary>
    /// Loads all command handlers discovered in the specified assembly.
    /// </summary>
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

    /// <summary>
    /// Loads all command handlers discovered in the specified assemblies.
    /// </summary>
    public CommandHandlersCollection LoadFromAssemblies(IEnumerable<Assembly> assemblies)
    {
        foreach (Assembly assembly in assemblies)
        {
            LoadFromAssembly(assembly);
        }

        return this;
    }
}
