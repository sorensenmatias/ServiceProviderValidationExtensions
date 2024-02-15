using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceProviderIronedValidation.Internal;

namespace ServiceProviderIronedValidation;

public static class HostBuilderExtensions{
    public static IHostBuilder UseServiceProviderExtendedValidation(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseServiceProviderFactory(new ExtendedValidationServiceProviderFactory(new DefaultServiceProviderFactory()));
        return hostBuilder;
    }

    public static HostApplicationBuilder ConfigureContainerWithServiceProviderExtendedValidation(this HostApplicationBuilder hostApplicationBuilder,
        Action<IServiceCollection>? configure = null)
    {
        hostApplicationBuilder.ConfigureContainer(new ExtendedValidationServiceProviderFactory(new DefaultServiceProviderFactory()), configure);
        return hostApplicationBuilder;
    }
}