using Microsoft.Extensions.DependencyInjection;
using ServiceProviderValidationExtensions.Internal;

namespace ServiceProviderValidationExtensions;

public sealed class ReportConfigurer : IReportConfigurer
{
    private Action<DuplicateServiceContent>? _duplicateServiceAction;
    private readonly IList<Type> _duplicateServiceExclusions = new List<Type>();
    
    public ReportConfigurer OnDuplicateService(Action<DuplicateServiceContent> action,
        Action<ReportingBuilderDuplicateServiceConfiguration>? configuration = null)
    {
        _duplicateServiceAction = action;
        if (configuration is not null)
        {
            configuration(new ReportingBuilderDuplicateServiceConfiguration(this));
        }
        
        return this;
    }

    public sealed class ReportingBuilderDuplicateServiceConfiguration
    {
        private readonly ReportConfigurer _reportConfigurer;

        internal ReportingBuilderDuplicateServiceConfiguration(ReportConfigurer reportConfigurer)
        {
            _reportConfigurer = reportConfigurer;
        }

        public ReportingBuilderDuplicateServiceConfiguration Except<T>()
        {
            _reportConfigurer._duplicateServiceExclusions.Add(typeof(T));
            return this;
        }

        public ReportingBuilderDuplicateServiceConfiguration Except(Type type)
        {
            _reportConfigurer._duplicateServiceExclusions.Add(type);
            return this;
        }
    }

    public void Report(IServiceCollection serviceCollection)
    {
        ReportDuplicateServices(serviceCollection);
    }

    private void ReportDuplicateServices(IServiceCollection serviceCollection)
    {
        if (_duplicateServiceAction is null)
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

            var duplicateServiceContent = new DuplicateServiceContent(new TypeInfo(groupItem.Key), implementationTypes);
            _duplicateServiceAction(duplicateServiceContent);
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
