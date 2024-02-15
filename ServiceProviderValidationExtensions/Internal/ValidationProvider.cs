using Microsoft.Extensions.DependencyInjection;

namespace ServiceProviderIronedValidation.Internal;

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
            .ToList();

        foreach (var exclusiveRegistration in exclusiveRegistrations)
        {
            var registrations = exclusiveRegistration
                .Select(t => t.service)
                .Distinct()
                .Select(service => (service, registrations: serviceProvider.GetServices(service).Count()))
                .ToList();

            var combinedSum = registrations.Sum(r => r.registrations);

            if (combinedSum > 1)
            {
                yield return
                    $"Implementation {exclusiveRegistration.Key.Name} is exclusive, but is registered {combinedSum} times: {string.Join(", ", registrations.Select(t => $"{t.service.Name}({t.registrations})"))}";
            }
        }
    }

}