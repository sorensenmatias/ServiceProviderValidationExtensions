using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceProviderValidationExtensions.Hosting.Internal;

namespace ServiceProviderValidationExtensions.Hosting;

public static class HostBuilderExtensions{
    public static IHostBuilder UseServiceProviderExtendedValidation(this IHostBuilder hostBuilder, ReportingBuilder? reportingBuilder = null)
    {
        hostBuilder.UseServiceProviderFactory(new ExtendedValidationServiceProviderFactory(new DefaultServiceProviderFactory(), reportingBuilder));
        return hostBuilder;
    }

    public static HostApplicationBuilder ConfigureContainerWithServiceProviderExtendedValidation(this HostApplicationBuilder hostApplicationBuilder,
        ReportingBuilder? reportingBuilder = null,
        Action<IServiceCollection>? configure = null)
    {
        hostApplicationBuilder.ConfigureContainer(new ExtendedValidationServiceProviderFactory(new DefaultServiceProviderFactory(), reportingBuilder), configure);
        return hostApplicationBuilder;
    }
}