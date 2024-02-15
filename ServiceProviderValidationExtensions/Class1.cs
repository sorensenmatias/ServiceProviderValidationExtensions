using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ServiceProviderValidationExtensions
{
    public class Class1
    {

    }

    public class Program
    {
        public static void Main(string[] args)
        {
            //CreateHostBuilder(args).Build().UseServiceDescriptionCheck().Run();

        }

        //public static IHostBuilder CreateHostBuilder(string[] args) =>
        //    Host.CreateDefaultBuilder(args)
        //        .ConfigureWebHostDefaults(webBuilder =>
        //        {
        //            webBuilder.UseStartup<Startup>();
        //        })
        //        .ConfigureServiceDescriptionCheck();
    }

    public static class ServiceDescription
    {
        private static List<IGrouping<Type, ServiceDescriptor>> Descriptors;

        public static IHostBuilder ConfigureServiceDescriptionCheck(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hbc, services) =>
            {
                Descriptors = services.Where(i => !i.ServiceType.Assembly.FullName.Contains("Microsoft"))
                    .GroupBy(p => p.ServiceType)
                    .Where(x => x.Count() > 1).ToList();

            });

            return hostBuilder;
        }

        public static IHost UseServiceDescriptionCheck(this IHost host)
        {
            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            Descriptors.ForEach(item =>
            {
                var count = item.Count();
                logger.LogWarning("Service of type {Key} has been registered {count} times", item.Key, count);
            });

            return host;
        }
    }

    internal sealed class ExtendedValidationServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
    {
        private readonly DefaultServiceProviderFactory _defaultServiceProviderFactory;

        public ExtendedValidationServiceProviderFactory(DefaultServiceProviderFactory defaultServiceProviderFactory)
        {
            _defaultServiceProviderFactory = defaultServiceProviderFactory;
        }

        public IServiceCollection CreateBuilder(IServiceCollection services)
        {
            return _defaultServiceProviderFactory.CreateBuilder(services);
        }

        public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
        {
            var serviceProvider = _defaultServiceProviderFactory.CreateServiceProvider(containerBuilder);
            serviceProvider.Validate();
            return serviceProvider;
        }
    }

    public interface IExclusiveRegistration
    {
    }

    public sealed class ExclusiveRegistration<TService> : IExclusiveRegistration
        where TService : class
    {
    }


    public static class Ext
    {
        public static IServiceCollection AddSingletonExclusive<TService>(
            this IServiceCollection services) where TService : class
        {
            services.AddSingleton<TService>();
            RegisterExclusive<TService>(services);
            return services;
        }

        public static IServiceCollection AddSingletonExclusive<TService, TImplementation>(this IServiceCollection services)
            where TService : class 
            where TImplementation : class, TService
        {
            services.AddSingleton<TService, TImplementation>();
            RegisterExclusive<TService>(services);
            return services;
        }

        private static void RegisterExclusive<TService>(IServiceCollection services)
            where TService : class
        {
            services.AddSingleton<IExclusiveRegistration, ExclusiveRegistration<TService>>();
        }
        
        public static ServiceProvider BuildServiceProviderWithValidation(this IServiceCollection serviceCollection)
        {
            var serviceProvider = serviceCollection.BuildServiceProvider();
            serviceProvider.Validate();
            return serviceProvider;
        }

        public static IHostBuilder UseServiceProviderExtendedValidation(this IHostBuilder hostBuilder)
        {
            hostBuilder.UseServiceProviderFactory(new ExtendedValidationServiceProviderFactory(new DefaultServiceProviderFactory()));
            return hostBuilder;
        }

        public static HostApplicationBuilder ConfigureContainerWithServiceProviderExtendedValidation(this HostApplicationBuilder hostApplicationBuilder, 
            Action<IServiceCollection>? configure = null)
        {
            hostApplicationBuilder.ConfigureContainer(new ExtendedValidationServiceProviderFactory(new DefaultServiceProviderFactory()), configure);
            return hostApplicationBuilder;
        }

        public static void Validate(this IServiceProvider serviceProvider)
        {
            var errors = GetExclusiveRegistrationErrors(serviceProvider).ToList();

            if (errors.Any())
            {
                throw new ServiceProviderValidationException(errors);
            }
        }

        private static IEnumerable<string> GetExclusiveRegistrationErrors(IServiceProvider serviceProvider)
        {
            var exclusiveRegistrations = serviceProvider
                .GetServices<IExclusiveRegistration>()
                .Select(er => er.GetType().GetGenericArguments().Single())
                .GroupBy(t => t)
                .Select(g => g.First());

            foreach (var exclusiveRegistration in exclusiveRegistrations)
            {
                var serviceRegistrationsCount = serviceProvider.GetServices(exclusiveRegistration).Count();

                if (serviceRegistrationsCount > 1)
                {
                    yield return $"Service {exclusiveRegistration.Name} is exclusive, but is registered {serviceRegistrationsCount} times.";
                }
            }
        }
    }

    public sealed class MultipleExclusiveRegistrationsException : Exception
    {
        public MultipleExclusiveRegistrationsException(Type type) : base($"The type {type.FullName} was already registered as exclusive.")
        {
        }
    }

    public sealed class ServiceProviderValidationException : Exception
    {
        public ServiceProviderValidationException(IReadOnlyCollection<string> errors)
        {
            Message =
                $"ServiceProvider validation failed with the following errors:{Environment.NewLine}{Environment.NewLine}{string.Join(Environment.NewLine, errors)}";
        }

        public override string Message { get; }
    }
}