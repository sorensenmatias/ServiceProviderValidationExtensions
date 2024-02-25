using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ServiceProviderValidationExtensions;

namespace Tests;

public class ReportingTests
{
    [Fact]
    public void ServiceRegisteredTwice()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IMyService, MyService>();
        serviceCollection.AddSingleton<IMyService, MyService2>();

        var duplicateServiceOutput = new List<string>();

        var reportingBuilder = new ReportingBuilder().OnDuplicateService(ds => duplicateServiceOutput.Add(
            $"{ds.ServiceType.DisplayName} is registered {ds.ImplementationTypes.Count} times"));

        serviceCollection.BuildServiceProviderWithValidation(reportingBuilder);

        duplicateServiceOutput.Should().ContainSingle().Which.Should().Be("Tests.ReportingTests+IMyService is registered 2 times");
    }

    [Fact]
    public void ServiceRegisteredTwiceOnceUsingLambda()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IMyService, MyService>();
        serviceCollection.AddSingleton<IMyService>(_ => new MyService2());

        var duplicateServiceOutput = new List<string>();

        var reportingBuilder = new ReportingBuilder().OnDuplicateService(ds => duplicateServiceOutput.Add(
            $"{ds.ServiceType.DisplayName} is registered {ds.ImplementationTypes.Count} times"));

        serviceCollection.BuildServiceProviderWithValidation(reportingBuilder);

        duplicateServiceOutput.Should().ContainSingle().Which.Should().Be("Tests.ReportingTests+IMyService is registered 2 times");
    }

    private interface IMyService
    {
    }

    private record MyService : IMyService;
    private record MyService2 : IMyService;
}