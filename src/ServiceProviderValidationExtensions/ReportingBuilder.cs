using Microsoft.Extensions.DependencyInjection;
using ServiceProviderValidationExtensions.Internal;

namespace ServiceProviderValidationExtensions;

public sealed class ReportingBuilder : IReportingBuilder
{
    private readonly IList<Action<DuplicateServiceContent>> _duplicateServiceActions = new List<Action<DuplicateServiceContent>>();
    private readonly IList<Type> _duplicateServiceExclusions = new List<Type>();
    
    public ReportingBuilder OnDuplicateService(Action<DuplicateServiceContent> action,
        Action<ReportingBuilderDuplicateServiceConfiguration>? configuration = null)
    {
        _duplicateServiceActions.Add(action);
        if (configuration is not null)
        {
            configuration(new ReportingBuilderDuplicateServiceConfiguration(this));
        }
        
        return this;
    }

    public sealed class ReportingBuilderDuplicateServiceConfiguration
    {
        private readonly ReportingBuilder _reportingBuilder;

        internal ReportingBuilderDuplicateServiceConfiguration(ReportingBuilder reportingBuilder)
        {
            _reportingBuilder = reportingBuilder;
        }

        public ReportingBuilderDuplicateServiceConfiguration Except<T>()
        {
            _reportingBuilder._duplicateServiceExclusions.Add(typeof(T));
            return this;
        }

        public ReportingBuilderDuplicateServiceConfiguration Except(Type type)
        {
            _reportingBuilder._duplicateServiceExclusions.Add(type);
            return this;
        }
    }

    public void Report(IServiceCollection serviceCollection)
    {
        ReportDuplicateServices(serviceCollection);
    }

    private void ReportDuplicateServices(IServiceCollection serviceCollection)
    {
        if (!_duplicateServiceActions.Any())
        {
            return;
        }

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

            foreach (var action in _duplicateServiceActions)
            {
                action(new DuplicateServiceContent(new TypeInfo(groupItem.Key), implementationTypes));
            }
        }
    }

    public sealed class DuplicateServiceContent
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
