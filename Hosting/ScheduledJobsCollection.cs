using System.Reflection;

using DioRed.Vermilion.Jobs;
using DioRed.Vermilion.Messages;

namespace DioRed.Vermilion.Hosting;

public class ScheduledJobsCollection(IServiceProvider services)
{
    internal List<IScheduledJob> ScheduledJobs { get; } = [];

    public ScheduledJobsCollection Add(IScheduledJob scheduledJob)
    {
        ScheduledJobs.Add(scheduledJob);

        return this;
    }

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

    public ScheduledJobsCollection Add<T>()
        where T : IScheduledJob
    {
        return Add(TypeResolver.Resolve<T>(services));
    }

    public ScheduledJobsCollection LoadFromEntryAssembly()
    {
        return LoadFromAssembly(Assembly.GetEntryAssembly()!);
    }

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

    public ScheduledJobsCollection LoadFromAssemblies(IEnumerable<Assembly> assemblies)
    {
        foreach (Assembly assembly in assemblies)
        {
            LoadFromAssembly(assembly);
        }

        return this;
    }
}