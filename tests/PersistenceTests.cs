using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ServiceProviderValidationExtensions.Internal;

namespace ServiceProviderValidationExtensions.Tests;

public class PersistenceTests
{
    [Fact]
    public void ValidationRegistrationShouldResideAtIndex0()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IMyService, MyService>(ServiceValidation.ExclusiveService);
        serviceCollection[0].Should().BeOfType<ServiceDescriptor>().Which.ImplementationInstance.Should().BeOfType<ValidationRegistrations>();

        //Move ValidationRegistrations away from index 0
        serviceCollection.Insert(0, ServiceDescriptor.Singleton<IMyService>(new MyService()));

        //This registration should move ValidationRegistrations to index 0
        serviceCollection.AddSingleton<IMyService, MyService>(ServiceValidation.ExclusiveService);
        serviceCollection[0].Should().BeOfType<ServiceDescriptor>().Which.ImplementationInstance.Should().BeOfType<ValidationRegistrations>();
    }

    private interface IMyService
    {
    }

    private record MyService : IMyService;
}
