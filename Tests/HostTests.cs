using System.Collections;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceProviderValidationExtensions;

namespace Tests
{
    public class HostTests
    {
        public class HostBuilders : IEnumerable<Func<Action<IServiceCollection>, IHost>>
        {
            public IEnumerator<Func<Action<IServiceCollection>, IHost>> GetEnumerator()
            {
                {
                    var defaultHostBuilder = (Action<IServiceCollection> configureServices) => Host.CreateDefaultBuilder()
                    .UseServiceProviderExtendedValidation()
                    .ConfigureServices(configureServices)
                    .Build();

                    yield return defaultHostBuilder;
                }
                {
                    var applicationBuilder = (Action<IServiceCollection> configureServices) =>
                    {
                        var applicationBuilder = Host.CreateApplicationBuilder().ConfigureContainerWithServiceProviderExtendedValidation();
                        configureServices(applicationBuilder.Services);
                        return applicationBuilder.Build();
                    };
                    
                    yield return applicationBuilder;
                }

            
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }


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