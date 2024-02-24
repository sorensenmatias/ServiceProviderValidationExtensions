using Microsoft.Extensions.DependencyInjection;

namespace ServiceProviderValidationExtensions.Hosting.Internal;

internal sealed class ExtendedValidationServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
{
    private readonly DefaultServiceProviderFactory _defaultServiceProviderFactory;

    public ExtendedValidationServiceProviderFactory(DefaultServiceProviderFactory defaultServiceProviderFactory)
    {
        _defaultServiceProviderFactory = defaultServiceProviderFactory;
    }

    public IServiceCollection CreateBuilder(IServiceCollection services)
    {
        return _defaultServiceProviderFactory.CreateBuilder(services);
    }

    public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
    {
        var serviceProvider = _defaultServiceProviderFactory.CreateServiceProvider(containerBuilder);
        serviceProvider.Validate();
        return serviceProvider;
    }
}