using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ServiceProviderValidationExtensions
{
    public static class Ext
    {
        public static IServiceCollection AddSingletonExclusive<TService>(
            this IServiceCollection services) where TService : class
        {
            services.AddSingleton<TService>();
            RegisterExclusive<TService>(services);
            return services;
        }

        public static IServiceCollection AddSingletonExclusive<TService, TImplementation>(this IServiceCollection services)
            where TService : class 
            where TImplementation : class, TService
        {
            services.AddSingleton<TService, TImplementation>();
            RegisterExclusive<TService>(services);
            return services;
        }

        private static void RegisterExclusive<TService>(IServiceCollection services)
            where TService : class
        {
            var validationRegistrations = EnsureValidationRegistrations(services);
            validationRegistrations.RegisterExclusive<TService>();
        }

        private static ValidationRegistrations? TryGetValidationRegistrationsAtExpectedIndex(IServiceCollection services)
        {
            return services[0].ImplementationInstance as ValidationRegistrations;
        }

        private static ValidationRegistrations? SearchForValidationRegistrations(IServiceCollection services, out ServiceDescriptor? serviceDescriptor)
        {
            serviceDescriptor = services.FirstOrDefault(sd => sd.ImplementationInstance is ValidationRegistrations);
            return (ValidationRegistrations?)serviceDescriptor?.ImplementationInstance;
        }

        private static ValidationRegistrations EnsureValidationRegistrations(IServiceCollection services)
        {
            if (services.Count == 0)
            {
                return CreateAndPersistInstance(services);
            }

            var validationRegistrations = TryGetValidationRegistrationsAtExpectedIndex(services);
            if (validationRegistrations is not null)
            {
                return validationRegistrations;
            }

            validationRegistrations = SearchForValidationRegistrations(services, out var serviceDescriptor);
            if (validationRegistrations is not null)
            {
                // Move instance to index 0
                services.Remove(serviceDescriptor);
                services.Insert(0, serviceDescriptor);

                return validationRegistrations;
            }

            return CreateAndPersistInstance(services);

            static ValidationRegistrations CreateAndPersistInstance(IServiceCollection services)
            {
                var validationRegistrations = new ValidationRegistrations();
                services.Insert(0, new ServiceDescriptor(typeof(ValidationRegistrations), validationRegistrations));
                return validationRegistrations;
            }
        }

        

        private class ValidationRegistrations
        {
            private readonly List<Type> _exclusives = new();

            public IReadOnlyCollection<Type> Exclusives => _exclusives;

            public void RegisterExclusive<TService>()
            {
                _exclusives.Add(typeof(TService));
            }
        }

        public static ServiceProvider BuildServiceProviderWithValidation(this IServiceCollection serviceCollection)
        {
            var serviceProvider = serviceCollection.BuildServiceProvider();
            serviceProvider.Validate();
            return serviceProvider;
        }

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

        public static void Validate(this IServiceProvider serviceProvider)
        {
            var errors = GetExclusiveRegistrationErrors(serviceProvider).ToList();

            if (errors.Any())
            {
                throw new ServiceProviderValidationException(errors);
            }
        }

        private static IEnumerable<string> GetExclusiveRegistrationErrors(IServiceProvider serviceProvider)
        {
            var validationRegistrations = serviceProvider.GetService<ValidationRegistrations>();
            if (validationRegistrations is null)
            {
                yield break;
            }

            var exclusiveRegistrations = validationRegistrations.Exclusives
                .GroupBy(t => t)
                .Select(g => g.First());

            foreach (var exclusiveRegistration in exclusiveRegistrations)
            {
                var serviceRegistrationsCount = serviceProvider.GetServices(exclusiveRegistration).Count();

                if (serviceRegistrationsCount > 1)
                {
                    yield return $"Service {exclusiveRegistration.Name} is exclusive, but is registered {serviceRegistrationsCount} times.";
                }
            }
        }
    }

    public sealed class MultipleExclusiveRegistrationsException : Exception
    {
        public MultipleExclusiveRegistrationsException(Type type) : base($"The type {type.FullName} was already registered as exclusive.")
        {
        }
    }

    public sealed class ServiceProviderValidationException : Exception
    {
        public ServiceProviderValidationException(IReadOnlyCollection<string> errors)
        {
            Message =
                $"ServiceProvider validation failed with the following errors:{Environment.NewLine}{Environment.NewLine}{string.Join(Environment.NewLine, errors)}";
        }

        public override string Message { get; }
    }
}