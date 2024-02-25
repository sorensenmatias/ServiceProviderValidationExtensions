using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceProviderValidationExtensions.Hosting.Internal;

namespace ServiceProviderValidationExtensions.Hosting;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseServiceProviderExtendedValidation(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseServiceProviderFactory(new ExtendedValidationServiceProviderFactory(new DefaultServiceProviderFactory(), null));
        return hostBuilder;
    }

    public static IHostBuilder UseServiceProviderExtendedValidation(this IHostBuilder hostBuilder, Action<ReportingBuilder> configureReporting)
    {
        var reportingBuilder = new ReportingBuilder();
        configureReporting(reportingBuilder);
        hostBuilder.UseServiceProviderFactory(new ExtendedValidationServiceProviderFactory(new DefaultServiceProviderFactory(), reportingBuilder));
        return hostBuilder;
    }

    public static HostApplicationBuilder ConfigureContainerWithServiceProviderExtendedValidation(this HostApplicationBuilder hostApplicationBuilder,
        Action<IServiceCollection>? configure = null)
    {
        hostApplicationBuilder.ConfigureContainer(new ExtendedValidationServiceProviderFactory(new DefaultServiceProviderFactory(), null), configure);
        return hostApplicationBuilder;
    }

    public static HostApplicationBuilder ConfigureContainerWithServiceProviderExtendedValidation(this HostApplicationBuilder hostApplicationBuilder,
        Action<ReportingBuilder> configureReporting,
        Action<IServiceCollection>? configure = null)
    {
        var reportingBuilder = new ReportingBuilder();
        configureReporting(reportingBuilder);
        hostApplicationBuilder.ConfigureContainer(new ExtendedValidationServiceProviderFactory(new DefaultServiceProviderFactory(), reportingBuilder), configure);
        return hostApplicationBuilder;
    }
}
