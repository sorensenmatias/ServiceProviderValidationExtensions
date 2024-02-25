using Microsoft.Extensions.DependencyInjection;

namespace ServiceProviderValidationExtensions;

public interface IReportingBuilder
{
    void Report(IServiceCollection serviceCollection);
}

public class ReportingBuilder : ReportingBuilder.IReportingBuilderDuplicateService
{
    private readonly IList<Action<DuplicateServiceContent>> _duplicateService = new List<Action<DuplicateServiceContent>>();
    private readonly IList<Type> _duplicateServiceExclusions = new List<Type>();

    public IReportingBuilderDuplicateService Except<T>()
    {
        _duplicateServiceExclusions.Add(typeof(T));
        return this;
    }

    public IReportingBuilderDuplicateService Except(Type type)
    {
        _duplicateServiceExclusions.Add(type);
        return this;
    }

    public IReportingBuilderDuplicateService OnDuplicateService(Action<DuplicateServiceContent> action)
    {
        _duplicateService.Add(action);
        return this;
    }

    public void Report(IServiceCollection serviceCollection)
    {
        if (_duplicateService.Any())
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

                    if (duplicateServiceExclusion.IsGenericType)
                    {
                        if (sd.ServiceType.IsDerivedFromGenericParent(duplicateServiceExclusion))
                        {
                            return false;
                        }
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

            foreach (var action in _duplicateService)
            {
                action(new DuplicateServiceContent(new TypeInfo(groupItem.Key), implementationTypes));
            }
        }
    }

    public interface IReportingBuilderDuplicateService : IReportingBuilder
    {
        IReportingBuilderDuplicateService Except<T>();
        IReportingBuilderDuplicateService Except(Type type);
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
