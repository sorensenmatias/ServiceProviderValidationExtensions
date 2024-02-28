using Microsoft.Extensions.DependencyInjection;

namespace ServiceProviderValidationExtensions;

public interface IReportConfigurer
{
    void Report(IServiceCollection serviceCollection);
}

public class ReportConfigurer : ReportConfigurer.IReportConfigurerDuplicateService
{
    private Action<DuplicateServiceContent>? _duplicateService;
    private readonly IList<Type> _duplicateServiceExclusions = new List<Type>();

    public IReportConfigurerDuplicateService Except<T>()
    {
        _duplicateServiceExclusions.Add(typeof(T));
        return this;
    }

    public IReportConfigurerDuplicateService Except(Type type)
    {
        _duplicateServiceExclusions.Add(type);
        return this;
    }

    public IReportConfigurerDuplicateService OnDuplicateService(Action<DuplicateServiceContent> action)
    {
        _duplicateService = action;
        return this;
    }

    public void Report(IServiceCollection serviceCollection)
    {
        if (_duplicateService is not null)
        {
            ReportDuplicates(serviceCollection);
        }
    }

    private void ReportDuplicates(IServiceCollection serviceCollection)
    {
        var group = serviceCollection
            .Where(sd =>
            {
                foreach (var duplicateServiceExclusion in _duplicateServiceExclusions)
                {
                    if (sd.ServiceType == duplicateServiceExclusion)
                    {
                        return false;
                    }

                    if (!duplicateServiceExclusion.IsGenericType)
                    {
                        continue;
                    }

                    if (sd.ServiceType.IsDerivedFromGenericParent(duplicateServiceExclusion))
                    {
                        return false;
                    }
                }

                return true;
            })
            .GroupBy(sd => sd.ServiceType);

        foreach (var groupItem in group.Where(g => g.Count() > 1))
        {
            var implementationTypes = groupItem
                .Select(sd => sd.ImplementationType)
                .Select(t => new TypeInfo(t))
                .ToList();

            _duplicateService?.Invoke(new DuplicateServiceContent(new TypeInfo(groupItem.Key), implementationTypes));
        }
    }

    public interface IReportConfigurerDuplicateService : IReportConfigurer
    {
        IReportConfigurerDuplicateService Except<T>();
        IReportConfigurerDuplicateService Except(Type type);
    }

    public class DuplicateServiceContent
    {
        public DuplicateServiceContent(TypeInfo serviceType, IReadOnlyCollection<TypeInfo> implementationTypes)
        {
            ServiceType = serviceType;
            ImplementationTypes = implementationTypes;
        }

        public TypeInfo ServiceType { get; }

        public IReadOnlyCollection<TypeInfo> ImplementationTypes { get; }
    }
}

public static class TypeExtensions
{
    public static bool IsDerivedFromGenericParent(this Type? type, Type parentType)
    {
        if (!parentType.IsGenericType)
        {
            throw new ArgumentException("type must be generic", nameof(parentType));
        }
        if (type == null || type == typeof(object))
        {
            return false;
        }
        if (type.IsGenericType && type.GetGenericTypeDefinition() == parentType)
        {
            return true;
        }
        return type.BaseType.IsDerivedFromGenericParent(parentType)
               || type.GetInterfaces().Any(t => t.IsDerivedFromGenericParent(parentType));
    }
}
