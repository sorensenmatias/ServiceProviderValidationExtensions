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

        var searchForValidationRegistrations = SearchForValidationRegistrations(services);
        if (searchForValidationRegistrations is null)
        {
            return CreateAndPersistInstance(services);
        }

        // Move instance to index 0
        services.Remove(searchForValidationRegistrations.Value.serviceDescriptor);
        services.Insert(0, searchForValidationRegistrations.Value.serviceDescriptor);

        return searchForValidationRegistrations.Value.validationRegistrations;
    }

    private static ValidationRegistrations CreateAndPersistInstance(IServiceCollection services)
    {
        var validationRegistrations = new ValidationRegistrations();
        services.Insert(0, new ServiceDescriptor(typeof(ValidationRegistrations), validationRegistrations));
        return validationRegistrations;
    }

    private static ValidationRegistrations? TryGetValidationRegistrationsAtExpectedIndex(IServiceCollection services)
    {
        return services[0].ImplementationInstance as ValidationRegistrations;
    }


    private static (ValidationRegistrations validationRegistrations, ServiceDescriptor serviceDescriptor)? SearchForValidationRegistrations(IServiceCollection services)
    {
        var serviceDescriptor = services.FirstOrDefault(sd => sd.ImplementationInstance is ValidationRegistrations);
        if (serviceDescriptor is null)
        {
            return null;
        }

        var validationRegistrations = (ValidationRegistrations)serviceDescriptor.ImplementationInstance!;
        return (validationRegistrations, serviceDescriptor);
    }
}
