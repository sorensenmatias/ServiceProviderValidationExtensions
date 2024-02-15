using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ServiceProviderIronedValidation;

namespace Tests
{
    public class BuildServiceProviderWithValidationTests
    {
        [Fact]
        public void ExclusiveServiceRegistrationAndNormalServiceRegistration()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<IMyService, MyService>();
            serviceCollection.AddSingleton<IMyService, MyService>(ServiceValidation.ExclusiveService);

            var act = () => serviceCollection.BuildServiceProviderWithValidation();
            act.Should()
                .ThrowExactly<ServiceProviderValidationException>()
                .WithMessage("ServiceProvider validation failed with the following errors:\r\n\r\nService IMyService is exclusive, but is registered 2 times.");
        }

        [Fact]
        public void ExclusiveImplementationRegistrationAndNormalImplementationRegistration()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<IMyService, MyService>();
            serviceCollection.AddSingleton<IMyService, MyService>(ImplementationValidation.ExclusiveImplementation);

            var act = () => serviceCollection.BuildServiceProviderWithValidation();
            act.Should()
                .ThrowExactly<ServiceProviderValidationException>()
                .WithMessage("ServiceProvider validation failed with the following errors:\r\n\r\nImplementation MyService is exclusive, but is registered 2 times: IMyService(2)");
        }

        [Fact]
        public void MultipleExclusiveServiceRegistrations()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<IMyService, MyService>(ServiceValidation.ExclusiveService);
            serviceCollection.AddSingleton<IMyService, MyService>(ServiceValidation.ExclusiveService);

            var act = () => serviceCollection.BuildServiceProviderWithValidation();
            act.Should()
                .ThrowExactly<ServiceProviderValidationException>()
                .WithMessage("ServiceProvider validation failed with the following errors:\r\n\r\nService IMyService is exclusive, but is registered 2 times.");
        }

        [Fact]
        public void MultipleExclusiveImplementationRegistrations()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<IMyService, MyService>(ImplementationValidation.ExclusiveImplementation);
            serviceCollection.AddSingleton<IMyService, MyService>(ImplementationValidation.ExclusiveImplementation);

            var act = () => serviceCollection.BuildServiceProviderWithValidation();
            act.Should()
                .ThrowExactly<ServiceProviderValidationException>()
                .WithMessage("ServiceProvider validation failed with the following errors:\r\n\r\nImplementation MyService is exclusive, but is registered 2 times: IMyService(2)");
        }

        public interface IMyService
        {
            
        }

        public class MyService : IMyService
        {
            
        }
    }
}