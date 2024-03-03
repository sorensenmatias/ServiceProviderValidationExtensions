using Microsoft.Extensions.DependencyInjection;
using ServiceProviderValidationExtensions.Internal;

namespace ServiceProviderValidationExtensions;

public enum ServiceValidation
{
    /// <summary>
    ///     No service validation.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Register exclusive validation for the service, which means that not more than one registration for this exact
    ///     service type can be made.
    /// </summary>
    ExclusiveService = 1
}

public enum ImplementationValidation
{
    /// <summary>
    ///     No implementation validation.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Register exclusive validation for the implementation, which means that not more than one registration for this pair
    ///     of service type and implementation can be made.
    /// </summary>
    ExclusiveImplementation = 1
}

public static class ServiceCollectionValidationExtensions
{
    public static IServiceCollection AddSingleton<TService>(this IServiceCollection services,
        ServiceValidation serviceValidation = ServiceValidation.None)
        where TService : class
    {
        ServiceCollectionServiceExtensions.AddSingleton<TService>(services);
        services.RegisterExclusiveService<TService>(serviceValidation);
        return services;
    }

    public static IServiceCollection AddSingleton<TService, TImplementation>(this IServiceCollection services,
        ServiceValidation serviceValidation = ServiceValidation.None,
        ImplementationValidation implementationValidation = ImplementationValidation.None)
        where TService : class
        where TImplementation : class, TService
    {
        ServiceCollectionServiceExtensions.AddSingleton<TService, TImplementation>(services);
        services.RegisterExclusiveService<TService>(serviceValidation);
        services.RegisterExclusiveImplementation<TService, TImplementation>(implementationValidation);
        return services;
    }

    public static IServiceCollection AddSingleton<TService, TImplementation>(this IServiceCollection services,
        ImplementationValidation implementationValidation = ImplementationValidation.None)
        where TService : class
        where TImplementation : class, TService
    {
        ServiceCollectionServiceExtensions.AddSingleton<TService, TImplementation>(services);
        services.RegisterExclusiveImplementation<TService, TImplementation>(implementationValidation);
        return services;
    }

    public static IServiceCollection AddScoped<TService>(this IServiceCollection services,
        ServiceValidation serviceValidation = ServiceValidation.None)
        where TService : class
    {
        ServiceCollectionServiceExtensions.AddScoped<TService>(services);
        services.RegisterExclusiveService<TService>(serviceValidation);
        return services;
    }

    public static IServiceCollection AddScoped<TService, TImplementation>(this IServiceCollection services,
        ServiceValidation serviceValidation = ServiceValidation.None,
        ImplementationValidation implementationValidation = ImplementationValidation.None)
        where TService : class
        where TImplementation : class, TService
    {
        ServiceCollectionServiceExtensions.AddScoped<TService, TImplementation>(services);
        services.RegisterExclusiveService<TService>(serviceValidation);
        services.RegisterExclusiveImplementation<TService, TImplementation>(implementationValidation);
        return services;
    }

    public static IServiceCollection AddScoped<TService, TImplementation>(this IServiceCollection services,
        ImplementationValidation implementationValidation = ImplementationValidation.None)
        where TService : class
        where TImplementation : class, TService
    {
        ServiceCollectionServiceExtensions.AddScoped<TService, TImplementation>(services);
        services.RegisterExclusiveImplementation<TService, TImplementation>(implementationValidation);
        return services;
    }

    public static IServiceCollection AddTransient<TService>(this IServiceCollection services,
        ServiceValidation serviceValidation = ServiceValidation.None)
        where TService : class
    {
        ServiceCollectionServiceExtensions.AddTransient<TService>(services);
        services.RegisterExclusiveService<TService>(serviceValidation);
        return services;
    }

    public static IServiceCollection AddTransient<TService, TImplementation>(this IServiceCollection services,
        ServiceValidation serviceValidation = ServiceValidation.None,
        ImplementationValidation implementationValidation = ImplementationValidation.None)
        where TService : class
        where TImplementation : class, TService
    {
        ServiceCollectionServiceExtensions.AddTransient<TService, TImplementation>(services);
        services.RegisterExclusiveService<TService>(serviceValidation);
        services.RegisterExclusiveImplementation<TService, TImplementation>(implementationValidation);
        return services;
    }

    public static IServiceCollection AddTransient<TService, TImplementation>(this IServiceCollection services,
        ImplementationValidation implementationValidation = ImplementationValidation.None)
        where TService : class
        where TImplementation : class, TService
    {
        ServiceCollectionServiceExtensions.AddTransient<TService, TImplementation>(services);
        services.RegisterExclusiveImplementation<TService, TImplementation>(implementationValidation);
        return services;
    }

    public static ServiceProvider BuildServiceProviderWithValidation(this IServiceCollection serviceCollection, IReportConfigurer? reportingBuilder = null)
    {
        var serviceProvider = ReportAndBuildAndValidate(serviceCollection, reportingBuilder, null);
        return serviceProvider;
    }

    public static ServiceProvider BuildServiceProviderWithValidation(this IServiceCollection serviceCollection, ServiceProviderOptions options, ReportConfigurer? reportingBuilder = null)
    {
        return ReportAndBuildAndValidate(serviceCollection, reportingBuilder, options);
    }

    private static ServiceProvider ReportAndBuildAndValidate(IServiceCollection serviceCollection, IReportConfigurer? reportingConfigurer, ServiceProviderOptions? options)
    {
        if (reportingConfigurer is not null)
        {
            reportingConfigurer.Report(serviceCollection);
        }

        ServiceProvider serviceProvider;
        if (options is not null)
        {
            serviceProvider = serviceCollection.BuildServiceProvider(options);
        }
        else
        {
            serviceProvider = serviceCollection.BuildServiceProvider();
        }

        serviceProvider.Validate();
        return serviceProvider;
    }

    public static void Validate(this IServiceProvider serviceProvider)
    {
        var errors = ValidationProvider.GetExclusiveServiceRegistrationErrors(serviceProvider)
            .Concat(ValidationProvider.GetExclusiveImplementationRegistrationErrors(serviceProvider))
            .ToList();

        if (errors.Any())
        {
            throw new ServiceProviderValidationException(errors);
        }
    }

    /// <summary>
    ///     Marks the <see cref="TService" /> as exclusive (only one registration can exist in the ServiceProvider).
    /// </summary>
    public static IServiceCollection SetExclusiveService<TService>(this IServiceCollection services)
        where TService : class
    {
        services.RegisterExclusiveService<TService>(ServiceValidation.ExclusiveService);
        return services;
    }

    private static void RegisterExclusiveService<TService>(this IServiceCollection services, ServiceValidation serviceValidation)
        where TService : class
    {
        switch (serviceValidation)
        {
            case ServiceValidation.None:
                break;
            case ServiceValidation.ExclusiveService:
                var validationRegistrations = Persistence.EnsureValidationRegistrations(services);
                validationRegistrations.Services.RegisterExclusive<TService>(serviceValidation);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(serviceValidation), serviceValidation, null);
        }
    }

    /// <summary>
    ///     Marks the (<see cref="TService" />,<see cref="TImplementation" />) combination as exclusive (only one registration
    ///     can exist in the ServiceProvider).
    /// </summary>
    public static void SetExclusiveImplementation<TService, TImplementation>(this IServiceCollection services)
        where TImplementation : class
    {
        services.RegisterExclusiveImplementation<TService, TImplementation>(ImplementationValidation.ExclusiveImplementation);
    }

    private static void RegisterExclusiveImplementation<TService, TImplementation>(this IServiceCollection services,
        ImplementationValidation implementationValidation) where TImplementation : class
    {
        switch (implementationValidation)
        {
            case ImplementationValidation.None:
                break;
            case ImplementationValidation.ExclusiveImplementation:
                var validationRegistrations = Persistence.EnsureValidationRegistrations(services);
                validationRegistrations.Implementations.RegisterExclusive<TService, TImplementation>(implementationValidation);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(implementationValidation), implementationValidation, null);
        }
    }
}
