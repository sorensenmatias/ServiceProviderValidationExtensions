using Microsoft.Extensions.DependencyInjection;

namespace ServiceProviderValidationExtensions;

public interface IReportingBuilder
{
    void Report(IServiceCollection serviceCollection);
}
