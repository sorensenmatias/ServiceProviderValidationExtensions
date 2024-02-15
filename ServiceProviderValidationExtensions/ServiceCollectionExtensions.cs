using Microsoft.Extensions.DependencyInjection;
using ServiceProviderValidationExtensions.Internal;

namespace ServiceProviderValidationExtensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSingletonExclusive<TService>(
            this IServiceCollection services) where TService : class
        {
            services.AddSingleton<TService>();
            services.RegisterExclusive<TService>();
            return services;
        }

        public static IServiceCollection AddSingletonExclusive<TService, TImplementation>(this IServiceCollection services)
            where TService : class 
            where TImplementation : class, TService
        {
            services.AddSingleton<TService, TImplementation>();
            services.RegisterExclusive<TService>();
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
            var errors = ValidationProvider.GetExclusiveRegistrationErrors(serviceProvider).ToList();

            if (errors.Any())
            {
                throw new ServiceProviderValidationException(errors);
            }
        }

        internal static void RegisterExclusive<TService>(this IServiceCollection services)
            where TService : class
        {
            var validationRegistrations = Persistence.EnsureValidationRegistrations(services);
            validationRegistrations.RegisterExclusive<TService>();
        }
    }
}