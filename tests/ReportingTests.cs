using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceProviderValidationExtensions.Tests;

public class ReportingTests
{
    [Fact]
    public void ServiceRegisteredTwice()
    {
        var serviceCollection = new ServiceCollection();

        ServiceCollectionServiceExtensions.AddSingleton<IMyService, MyService>(serviceCollection);
        ServiceCollectionServiceExtensions.AddSingleton<IMyService, MyService2>(serviceCollection);

        var duplicateServiceOutput = new List<string>();

        var reportingBuilder = new ReportConfigurer().OnDuplicateService(ds => duplicateServiceOutput.Add(
            $"{ds.ServiceType.DisplayName} is registered {ds.ImplementationTypes.Count} times"));

        serviceCollection.BuildServiceProviderWithValidation(reportingBuilder);

        duplicateServiceOutput.Should().ContainSingle().Which.Should().Be("ServiceProviderValidationExtensions.Tests.ReportingTests+IMyService is registered 2 times");
    }

    [Fact]
    public void ServiceRegisteredTwiceOnceUsingLambda()
    {
        var serviceCollection = new ServiceCollection();

        ServiceCollectionServiceExtensions.AddSingleton<IMyService, MyService>(serviceCollection);
        serviceCollection.AddSingleton<IMyService>(_ => new MyService2());

        var duplicateServiceOutput = new List<string>();

        var reportingBuilder = new ReportConfigurer().OnDuplicateService(ds => duplicateServiceOutput.Add(
            $"{ds.ServiceType.DisplayName} is registered {ds.ImplementationTypes.Count} times"));

        serviceCollection.BuildServiceProviderWithValidation(reportingBuilder);

        duplicateServiceOutput.Should().ContainSingle().Which.Should().Be("ServiceProviderValidationExtensions.Tests.ReportingTests+IMyService is registered 2 times");
    }

    private interface IMyService
    {
    }

    private record MyService : IMyService;

    private record MyService2 : IMyService;
}
