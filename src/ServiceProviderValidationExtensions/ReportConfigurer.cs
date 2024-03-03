using Microsoft.Extensions.DependencyInjection;
using ServiceProviderValidationExtensions.Internal;

namespace ServiceProviderValidationExtensions;

public sealed class ReportConfigurer : IReportConfigurer
{
    private readonly IList<Type> _duplicateServiceExclusions = new List<Type>();
    private Action<DuplicateServiceContent>? _duplicateServiceAction;

    public void Report(IServiceCollection serviceCollection)
    {
        ReportDuplicateServices(serviceCollection);
    }

    public ReportConfigurer OnDuplicateService(Action<DuplicateServiceContent> action,
        Action<ReportingDuplicateServiceConfiguration>? configuration = null)
    {
        _duplicateServiceAction = action;
        if (configuration is not null)
        {
            configuration(new ReportingDuplicateServiceConfiguration(this));
        }

        return this;
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

    public sealed class ReportingDuplicateServiceConfiguration
    {
        private readonly ReportConfigurer _reportConfigurer;

        internal ReportingDuplicateServiceConfiguration(ReportConfigurer reportConfigurer)
        {
            _reportConfigurer = reportConfigurer;
        }

        public ReportingDuplicateServiceConfiguration Except<T>()
        {
            _reportConfigurer._duplicateServiceExclusions.Add(typeof(T));
            return this;
        }

        public ReportingDuplicateServiceConfiguration Except(Type type)
        {
            _reportConfigurer._duplicateServiceExclusions.Add(type);
            return this;
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
