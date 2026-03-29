using System.Reflection;

using DioRed.Vermilion.Jobs;
using DioRed.Vermilion.Messages;

namespace DioRed.Vermilion.Hosting;

/// <summary>
/// Collects scheduled jobs that will be started together with the bot.
/// </summary>
public class ScheduledJobsCollection(IServiceProvider services)
{
    internal List<IScheduledJob> ScheduledJobs { get; } = [];

    /// <summary>
    /// Adds the specified scheduled job instance.
    /// </summary>
    public ScheduledJobsCollection Add(IScheduledJob scheduledJob)
    {
        ScheduledJobs.Add(scheduledJob);

        return this;
    }

    /// <summary>
    /// Resolves and adds a scheduled job of the specified type.
    /// </summary>
    public ScheduledJobsCollection Add(Type type)
    {
        if (!typeof(IScheduledJob).IsAssignableFrom(type))
        {
            throw new ArgumentException(
                string.Format(
                    ExceptionMessages.TypeDoesntImplementTheInterface_2,
                    type.Name,
                    nameof(IScheduledJob)
                ),
                nameof(type)
            );
        }

        return Add((IScheduledJob)TypeResolver.Resolve(services, type));
    }

    /// <summary>
    /// Resolves and adds a scheduled job of type <typeparamref name="T"/>.
    /// </summary>
    public ScheduledJobsCollection Add<T>()
        where T : IScheduledJob
    {
        return Add(TypeResolver.Resolve<T>(services));
    }

    /// <summary>
    /// Loads all scheduled jobs from the entry assembly.
    /// </summary>
    public ScheduledJobsCollection LoadFromEntryAssembly()
    {
        return LoadFromAssembly(Assembly.GetEntryAssembly()!);
    }

    /// <summary>
    /// Loads all scheduled jobs from the specified assembly.
    /// </summary>
    public ScheduledJobsCollection LoadFromAssembly(Assembly assembly)
    {
        TypeInfo[] dailyJobs =
        [
            .. assembly.DefinedTypes
                .Where(typeInfo =>
                    typeInfo.IsClass &&
                    !typeInfo.IsAbstract &&
                    typeInfo.ImplementedInterfaces.Contains(typeof(IScheduledJob)))
        ];

        foreach (TypeInfo typeInfo in dailyJobs)
        {
            Add(typeInfo.AsType());
        }

        return this;
    }

    /// <summary>
    /// Loads all scheduled jobs from the specified assemblies.
    /// </summary>
    public ScheduledJobsCollection LoadFromAssemblies(IEnumerable<Assembly> assemblies)
    {
        foreach (Assembly assembly in assemblies)
        {
            LoadFromAssembly(assembly);
        }

        return this;
    }
}
