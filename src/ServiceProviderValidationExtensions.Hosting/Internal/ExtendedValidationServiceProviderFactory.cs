using Microsoft.Extensions.DependencyInjection;

namespace ServiceProviderValidationExtensions.Hosting.Internal;

internal sealed class ExtendedValidationServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
{
    private readonly DefaultServiceProviderFactory _defaultServiceProviderFactory;
    private readonly ReportConfigurer? _reportingConfigurer;

    public ExtendedValidationServiceProviderFactory(DefaultServiceProviderFactory defaultServiceProviderFactory, ReportConfigurer? reportingConfigurer)
    {
        _defaultServiceProviderFactory = defaultServiceProviderFactory;
        _reportingConfigurer = reportingConfigurer;
    }

    public IServiceCollection CreateBuilder(IServiceCollection services)
    {
        return _defaultServiceProviderFactory.CreateBuilder(services);
    }

    public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
    {
        _reportingConfigurer?.Report(containerBuilder);
        var serviceProvider = _defaultServiceProviderFactory.CreateServiceProvider(containerBuilder);
        serviceProvider.Validate();
        return serviceProvider;
    }
}
