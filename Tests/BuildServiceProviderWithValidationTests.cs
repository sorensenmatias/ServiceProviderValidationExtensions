using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ServiceProviderValidationExtensions;

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
        public void ExclusiveServiceRegistrationAllowsRegistrationAsDifferentInterface()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<MyService>(ServiceValidation.ExclusiveService);
            serviceCollection.AddSingleton<IMyService, MyService>();

            var act = () => serviceCollection.BuildServiceProviderWithValidation();
            act.Should().NotThrow();
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
                .WithMessage("ServiceProvider validation failed with the following errors:\r\n\r\nImplementation MyService for service IMyService is exclusive, but is registered 2 times.");
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
                .WithMessage("ServiceProvider validation failed with the following errors:\r\n\r\nImplementation MyService for service IMyService is exclusive, but is registered 2 times.");
        }

        [Fact]
        public void BothExclusiveServiceAndImplementationRegistrations()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<IMyService, MyService>(ServiceValidation.ExclusiveService);
            serviceCollection.AddSingleton<IMyService, MyService>(ImplementationValidation.ExclusiveImplementation);

            var act = () => serviceCollection.BuildServiceProviderWithValidation();
            act.Should()
                .ThrowExactly<ServiceProviderValidationException>()
                .WithMessage("ServiceProvider validation failed with the following errors:\r\n\r\nService IMyService is exclusive, but is registered 2 times.\r\nImplementation MyService for service IMyService is exclusive, but is registered 2 times.");
        }

        [Fact]
        public void ExclusiveImplementationRegistrationForSameInterfaceIsAllowed()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<IMyProvider, MyProvider1>(ImplementationValidation.ExclusiveImplementation);
            serviceCollection.AddSingleton<IMyProvider, MyProvider2>(ImplementationValidation.ExclusiveImplementation);

            var act = () => serviceCollection.BuildServiceProviderWithValidation();
            act.Should().NotThrow();
        }

        public interface IMyService
        {
        }

        public class MyService : IMyService
        {
            
        }

        public interface IMyProvider
        {
            
        }

        public class MyProvider1 : IMyProvider
        {
            
        }

        public class MyProvider2 : IMyProvider
        {

        }
    }
}