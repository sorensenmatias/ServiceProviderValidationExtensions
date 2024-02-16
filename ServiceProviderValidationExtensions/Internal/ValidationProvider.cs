using Microsoft.Extensions.DependencyInjection;

namespace ServiceProviderValidationExtensions.Internal;

internal static class ValidationProvider
{
    internal static IEnumerable<string> GetExclusiveServiceRegistrationErrors(IServiceProvider serviceProvider)
    {
        var validationRegistrations = serviceProvider.GetService<ValidationRegistrations>();
        if (validationRegistrations is null)
        {
            yield break;
        }

        var exclusiveRegistrations = validationRegistrations.Services.Exclusives
            .GroupBy(t => t)
            .Select(g => g.First());

        foreach (var exclusiveRegistration in exclusiveRegistrations)
        {
            var registrationsCount = serviceProvider.GetServices(exclusiveRegistration).Count();

            if (registrationsCount > 1)
            {
                yield return $"Service {exclusiveRegistration.Name} is exclusive, but is registered {registrationsCount} times.";
            }
        }
    }

    internal static IEnumerable<string> GetExclusiveImplementationRegistrationErrors(IServiceProvider serviceProvider)
    {
        var validationRegistrations = serviceProvider.GetService<ValidationRegistrations>();
        if (validationRegistrations is null)
        {
            yield break;
        }

        var exclusiveRegistrations = validationRegistrations.Implementations.Exclusives
            .GroupBy(t => t.implementation)
            .Select(g => g.First());

        foreach (var exclusiveRegistration in exclusiveRegistrations)
        {
            var registrationsCount = serviceProvider
                .GetServices(exclusiveRegistration.service)
                .Count(o => o?.GetType() == exclusiveRegistration.implementation);

            if (registrationsCount > 1)
            {
                yield return
                    $"Implementation {exclusiveRegistration.implementation.Name} for service {exclusiveRegistration.service.Name} is exclusive, but is registered {registrationsCount} times.";
            }
        }
    }

}