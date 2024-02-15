using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ServiceProviderValidationExtensions;

namespace Tests
{
    public class BuildServiceProviderWithValidationTests
    {
        [Fact]
        public void ExclusiveRegistrationAndNormalRegistration()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingletonExclusive<IMyService, MyService>();
            serviceCollection.AddSingleton<IMyService, MyService>();

            var act = () => serviceCollection.BuildServiceProviderWithValidation();
            act.Should()
                .ThrowExactly<ServiceProviderValidationException>()
                .WithMessage("ServiceProvider validation failed with the following errors:\r\n\r\nService IMyService is exclusive, but is registered 2 times.");
        }

        [Fact]
        public void MultipleExclusiveRegistrations()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingletonExclusive<IMyService, MyService>();
            serviceCollection.AddSingletonExclusive<IMyService, MyService>();

            var act = () => serviceCollection.BuildServiceProviderWithValidation();
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