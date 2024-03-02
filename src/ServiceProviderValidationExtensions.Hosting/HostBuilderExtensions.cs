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

    public static IHostBuilder UseServiceProviderExtendedValidation(this IHostBuilder hostBuilder, Action<ReportConfigurer> configureReporting)
    {
        var reportingBuilder = new ReportConfigurer();
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
        Action<ReportConfigurer> configureReporting,
        Action<IServiceCollection>? configure = null)
    {
        var reportingBuilder = new ReportConfigurer();
        configureReporting(reportingBuilder);
        hostApplicationBuilder.ConfigureContainer(new ExtendedValidationServiceProviderFactory(new DefaultServiceProviderFactory(), reportingBuilder), configure);
        return hostApplicationBuilder;
    }
}
