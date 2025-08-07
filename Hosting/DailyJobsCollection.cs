using System.Reflection;

using DioRed.Vermilion.Jobs;
using DioRed.Vermilion.L10n;

namespace DioRed.Vermilion.Hosting;
public class DailyJobsCollection(IServiceProvider services)
{
    internal List<IDailyJob> DailyJobs { get; } = [];

    public DailyJobsCollection Add(IDailyJob dailyJob)
    {
        DailyJobs.Add(dailyJob);

        return this;
    }

    public DailyJobsCollection Add(Type type)
    {
        if (!typeof(IDailyJob).IsAssignableFrom(type))
        {
            throw new ArgumentException(
                string.Format(
                    ExceptionMessages.TypeDoesntImplementTheInterface_2,
                    type.Name,
                    nameof(IDailyJob)
                ),
                nameof(type)
            );
        }

        return Add((IDailyJob)TypeResolver.Resolve(services, type));
    }

    public DailyJobsCollection Add<T>()
        where T : IDailyJob
    {
        return Add(TypeResolver.Resolve<T>(services));
    }

    public DailyJobsCollection LoadFromEntryAssembly()
    {
        return LoadFromAssembly(Assembly.GetEntryAssembly()!);
    }

    public DailyJobsCollection LoadFromAssembly(Assembly assembly)
    {
        TypeInfo[] dailyJobs =
        [
            .. assembly.DefinedTypes
                .Where(typeInfo =>
                    typeInfo.IsClass &&
                    !typeInfo.IsAbstract &&
                    typeInfo.ImplementedInterfaces.Contains(typeof(IDailyJob)))
        ];

        foreach (TypeInfo typeInfo in dailyJobs)
        {
            Add(typeInfo.AsType());
        }

        return this;
    }

    public DailyJobsCollection LoadFromAssemblies(IEnumerable<Assembly> assemblies)
    {
        foreach (Assembly assembly in assemblies)
        {
            LoadFromAssembly(assembly);
        }

        return this;
    }
}