using Microsoft.Extensions.DependencyInjection;

namespace ServiceProviderValidationExtensions.Internal;

internal static class ValidationProvider
{
    internal static IEnumerable<string> GetExclusiveRegistrationErrors(IServiceProvider serviceProvider)
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