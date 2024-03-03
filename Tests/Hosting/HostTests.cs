using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceProviderValidationExtensions.Hosting;

namespace ServiceProviderValidationExtensions.Tests.Hosting;

public class HostTests
{
    [Fact]
    public void DefaultBuilder_TriggersValidation()
    {
        var applicationBuilder = Host.CreateDefaultBuilder()
            .UseServiceProviderExtendedValidation()
            .ConfigureServices(sc =>
            {
                ServiceCollectionServiceExtensions.AddSingleton<IMyService, MyService>(sc);
                sc.AddSingleton<IMyService, MyService>(ServiceValidation.ExclusiveService);
            });

        var act = () => applicationBuilder.Build();

        act.Should()
            .ThrowExactly<ServiceProviderValidationException>()
            .WithMessage("ServiceProvider validation failed with the following errors:\r\n\r\nService IMyService is exclusive, but is registered 2 times.");
    }

    [Fact]
    public void DefaultBuilder_ReportsDuplicate()
    {
        var duplicates = new List<string>();

        var applicationBuilder = Host.CreateDefaultBuilder()
            .UseServiceProviderExtendedValidation(rb => rb.OnDuplicateService(dsc => duplicates.Add($"{dsc.ServiceType.DisplayName} is registered 2 times")))
            .ConfigureServices(sc =>
            {
                ServiceCollectionServiceExtensions.AddSingleton<IMyService, MyService>(sc);
                ServiceCollectionServiceExtensions.AddSingleton<IMyService, MyService>(sc);
            });

        applicationBuilder.Build();

        duplicates.Should().Contain("ServiceProviderValidationExtensions.Tests.Hosting.HostTests+IMyService is registered 2 times");
    }

    [Fact]
    public void ApplicationBuilder_TriggersValidation()
    {
        var applicationBuilder = Host.CreateApplicationBuilder().ConfigureContainerWithServiceProviderExtendedValidation();

        ServiceCollectionServiceExtensions.AddSingleton<IMyService, MyService>(applicationBuilder.Services);
        applicationBuilder.Services.AddSingleton<IMyService, MyService>(ServiceValidation.ExclusiveService);

        var act = () => applicationBuilder.Build();

        act.Should()
            .ThrowExactly<ServiceProviderValidationException>()
            .WithMessage("ServiceProvider validation failed with the following errors:\r\n\r\nService IMyService is exclusive, but is registered 2 times.");
    }

    [Fact]
    public void ApplicationBuilder_BasicReporting()
    {
        var duplicates = new List<string>();

        var applicationBuilder = Host.CreateApplicationBuilder()
            .ConfigureContainerWithServiceProviderExtendedValidation(rb =>
                rb.OnDuplicateService(dsc =>
                    duplicates.Add($"{dsc.ServiceType.DisplayName} is registered 2 times")));


        ServiceCollectionServiceExtensions.AddSingleton<IMyService, MyService>(applicationBuilder.Services);
        ServiceCollectionServiceExtensions.AddSingleton<IMyService, MyService>(applicationBuilder.Services);

        applicationBuilder.Build();

        duplicates.Should().Contain("ServiceProviderValidationExtensions.Tests.Hosting.HostTests+IMyService is registered 2 times");
    }

#if NET8_0_OR_GREATER
    [Fact]
    public void EmptyApplicationBuilder_TriggersValidation()
    {
        var applicationBuilder = Host.CreateEmptyApplicationBuilder(null).ConfigureContainerWithServiceProviderExtendedValidation();

        ServiceCollectionServiceExtensions.AddSingleton<IMyService, MyService>(applicationBuilder.Services);
        applicationBuilder.Services.AddSingleton<IMyService, MyService>(ServiceValidation.ExclusiveService);

        var act = () => applicationBuilder.Build();

        act.Should()
            .ThrowExactly<ServiceProviderValidationException>()
            .WithMessage("ServiceProvider validation failed with the following errors:\r\n\r\nService IMyService is exclusive, but is registered 2 times.");
    }
#endif

    public interface IMyService
    {
    }

    public class MyService : IMyService
    {
    }
}
