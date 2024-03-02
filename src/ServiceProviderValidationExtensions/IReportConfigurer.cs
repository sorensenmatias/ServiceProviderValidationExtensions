using Microsoft.Extensions.DependencyInjection;

namespace ServiceProviderValidationExtensions;

public interface IReportConfigurer
{
    void Report(IServiceCollection serviceCollection);
}
