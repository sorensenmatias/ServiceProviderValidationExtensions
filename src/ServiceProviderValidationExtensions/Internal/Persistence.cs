using Microsoft.Extensions.DependencyInjection;

namespace ServiceProviderValidationExtensions.Internal;

internal static class Persistence
{
    internal static ValidationRegistrations EnsureValidationRegistrations(IServiceCollection services)
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

    private static ValidationRegistrations? TryGetValidationRegistrationsAtExpectedIndex(IServiceCollection services)
    {
        return services[0].ImplementationInstance as ValidationRegistrations;
    }

    private static ValidationRegistrations? SearchForValidationRegistrations(IServiceCollection services, out ServiceDescriptor? serviceDescriptor)
    {
        serviceDescriptor = services.FirstOrDefault(sd => sd.ImplementationInstance is ValidationRegistrations);
        return (ValidationRegistrations?)serviceDescriptor?.ImplementationInstance;
    }
}
