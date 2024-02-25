using Microsoft.Extensions.DependencyInjection;

namespace ServiceProviderValidationExtensions;

public class ReportingBuilder
{
    private readonly IList<Action<DuplicateServiceContent>> _duplicateService =
        new List<Action<DuplicateServiceContent>>();

    public ReportingBuilder OnDuplicateService(Action<DuplicateServiceContent> action)
    {
        _duplicateService.Add(action);
        return this;
    }

    internal void Report(IServiceCollection serviceCollection)
    {
        if (_duplicateService.Any())
        {
            ReportDuplicates(serviceCollection);
        }
    }

    private void ReportDuplicates(IServiceCollection serviceCollection)
    {
        var group = serviceCollection.GroupBy(sd => sd.ServiceType);

        foreach (var groupItem in group.Where(g => g.Count() > 1))
        {
            var implementationTypes = groupItem
                .Select(sd => sd.ImplementationType)
                .Select(t => new TypeInfo(t))
                .ToList();

            foreach (var action in _duplicateService)
            {
                action(new DuplicateServiceContent(new TypeInfo(groupItem.Key),
                    implementationTypes));
            }
        }
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
