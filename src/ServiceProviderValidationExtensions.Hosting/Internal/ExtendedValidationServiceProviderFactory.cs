using Microsoft.Extensions.DependencyInjection;

namespace ServiceProviderValidationExtensions.Hosting.Internal;

internal sealed class ExtendedValidationServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
{
    private readonly DefaultServiceProviderFactory _defaultServiceProviderFactory;
    private readonly ReportConfigurer? _reportingBuilder;

    public ExtendedValidationServiceProviderFactory(DefaultServiceProviderFactory defaultServiceProviderFactory, ReportConfigurer? reportingBuilder)
    {
        _defaultServiceProviderFactory = defaultServiceProviderFactory;
        _reportingBuilder = reportingBuilder;
    }

    public IServiceCollection CreateBuilder(IServiceCollection services)
    {
        return _defaultServiceProviderFactory.CreateBuilder(services);
    }

    public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
    {
        _reportingBuilder?.Report(containerBuilder);
        var serviceProvider = _defaultServiceProviderFactory.CreateServiceProvider(containerBuilder);
        serviceProvider.Validate();
        return serviceProvider;
    }
}
