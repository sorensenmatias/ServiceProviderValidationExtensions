using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceProviderValidationExtensions;
using ServiceProviderValidationExtensions.Hosting;

namespace Tests.Hosting
{
    public class HostTests
    {
        [Fact]
        public void DefaultBuilder_TriggersValidation()
        {
            var applicationBuilder = Host.CreateDefaultBuilder()
                .UseServiceProviderExtendedValidation()
                .ConfigureServices(sc =>
                {
                    sc.AddSingleton<IMyService, MyService>();
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
                    sc.AddSingleton<IMyService, MyService>();
                    sc.AddSingleton<IMyService, MyService>();
                });

            applicationBuilder.Build();

            duplicates.Should().Contain("Tests.Hosting.HostTests+IMyService is registered 2 times");
        }

        [Fact]
        public void ApplicationBuilder_TriggersValidation()
        {
            var applicationBuilder = Host.CreateApplicationBuilder().ConfigureContainerWithServiceProviderExtendedValidation();

            applicationBuilder.Services.AddSingleton<IMyService, MyService>();
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


            applicationBuilder.Services.AddSingleton<IMyService, MyService>();
            applicationBuilder.Services.AddSingleton<IMyService, MyService>();

            applicationBuilder.Build();

            duplicates.Should().Contain("Tests.Hosting.HostTests+IMyService is registered 2 times");
        }

        [Fact]
        public void EmptyApplicationBuilder_TriggersValidation()
        {
            var applicationBuilder = Host.CreateEmptyApplicationBuilder(null).ConfigureContainerWithServiceProviderExtendedValidation();

            applicationBuilder.Services.AddSingleton<IMyService, MyService>();
            applicationBuilder.Services.AddSingleton<IMyService, MyService>(ServiceValidation.ExclusiveService);

            var act = () => applicationBuilder.Build();

            act.Should()
                .ThrowExactly<ServiceProviderValidationException>()
                .WithMessage("ServiceProvider validation failed with the following errors:\r\n\r\nService IMyService is exclusive, but is registered 2 times.");
        }

        public interface IMyService
        {

        }

        public class MyService : IMyService
        {

        }
    }
}