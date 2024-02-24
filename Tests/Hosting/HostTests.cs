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