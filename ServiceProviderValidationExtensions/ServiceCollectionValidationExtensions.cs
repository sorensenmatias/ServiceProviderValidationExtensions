using Microsoft.Extensions.DependencyInjection;
using ServiceProviderIronedValidation.Internal;

namespace ServiceProviderIronedValidation
{
    public enum ServiceValidation
    {
        None = 0,
        ExclusiveService = 1
    }

    public enum ImplementationValidation
    {
        None = 0,
        ExclusiveImplementation = 1
    }

    public static class ServiceCollectionValidationExtensions
    {
        public static IServiceCollection AddSingleton<TService>(this IServiceCollection services, 
            ServiceValidation serviceValidation = ServiceValidation.None, 
            ImplementationValidation implementationValidation = ImplementationValidation.None) 
            where TService : class
        {
            ServiceCollectionServiceExtensions.AddSingleton<TService>(services);
            services.RegisterExclusiveService<TService>(serviceValidation);
            services.RegisterExclusiveImplementation<TService, TService>(implementationValidation);
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
            services.RegisterExclusiveImplementation<TService, TImplementation >(implementationValidation);
            return services;
        }

        public static ServiceProvider BuildServiceProviderWithValidation(this IServiceCollection serviceCollection)
        {
            var serviceProvider = serviceCollection.BuildServiceProvider();
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

        public static void RegisterExclusiveService<TService>(this IServiceCollection services, ServiceValidation serviceValidation)
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

        public static void RegisterExclusiveImplementation<TService, TImplementation>(this IServiceCollection services, ImplementationValidation implementationValidation)
            where TImplementation : class
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
}