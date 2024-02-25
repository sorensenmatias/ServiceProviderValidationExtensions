using Microsoft.Extensions.DependencyInjection;

namespace ServiceProviderValidationExtensions;

public class ReportingBuilder
{
    private readonly IList<Action<DuplicateServiceContent>> _duplicateService = new List<Action<DuplicateServiceContent>>();

    public ReportingBuilder OnDuplicateService(Action<DuplicateServiceContent> action)
    {
        _duplicateService.Add(action);
        return this;
    }

    public class DuplicateServiceContent
    {
        public DuplicateServiceContent(Type serviceType, IReadOnlyCollection<Type?> implementationTypes)
        {
            ServiceType = serviceType;
            ImplementationTypes = implementationTypes;
        }

        public Type ServiceType { get; }

        public IReadOnlyCollection<Type?> ImplementationTypes { get; }
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
            var implementationTypes = groupItem.Select(sd => sd.ImplementationType).ToList();

            foreach (var action in _duplicateService)
            {
                action(new DuplicateServiceContent(groupItem.Key, implementationTypes));
            }
        }
    }
}