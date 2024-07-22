# ServiceProviderValidationExtensions

This library enables more fine-grained control for registrations in ServiceProvider. 
These are especially useful for large complicated application.

## Installation

There are two separate NuGet packages supplied:

- `ServiceProviderValidationExtensions` which can be used with àny .NET app (think 'raw' ServiceProvider cases).
- `ServiceProviderValidationExtensions.Hosting` which is designed to be used together with ``Microsoft.Extensions.Hosting`` (think ServiceProviders used within ASP.NET) .

## Features

The following features are supplied:

### Exclusive service registration

A service can be registered as exclusive, meaning that not more than once occourence of it can exists in the ServiceProvider.
To register a service as exclusive, call `SetExclusiveService` on the `IServiceCollection`:

```
serviceCollection.SetExclusiveService<IMyService>()
```

Alternatively, overloads for the more common signatures of `AddSingleton`, `AddScoped` and `AddTransient` exists, e.g.:

```
serviceCollection.AddSingleton<MyService>(ServiceValidation.ExclusiveService);
serviceCollection.AddSingleton<IMyService, MyService>(ServiceValidation.ExclusiveService);
```

### Exclusive implementation registration

An implementation can be registered as exclusive, meaning that not more than once occourence of the combination of the type of service and the implementation can exists in the ServiceProvider.
To register a service as exclusive, call `SetExclusiveImplementation` on the `IServiceCollection`:

```
serviceCollection.SetExclusiveImplementation<IMyService, MyService>()
```

Alternatively, overloads for the more common signatures of `AddSingleton`, `AddScoped` and `AddTransient` exists, e.g.:

```
serviceCollection.AddSingleton<IMyService, MyService>(ServiceValidation.ExclusiveService);
```

### Reporting

The reporting functionality makes it possible to get insights into the registrations in the ServiceProvider.
It currently supports detecting duplicate service registrations.

```
void ConfigureReporting(ReportConfigurer rb)
{
    rb.OnDuplicateService(
        ds => Console.WriteLine($"{ds.ServiceType} is registered {dsc.ImplementationTypes.Count} times"),
        c => c.Except(typeof(IConfigureOptions<>)).Except<ILoggerProvider>());
}

Host.CreateDefaultBuilder()
    .UseServiceProviderExtendedValidation(ConfigureReporting)
    .Build();
```

## How to wire it up

There are two ways of actually invoking the validation, as described below.
In case any validation fails, a `ServiceProviderValidationException` is thrown.

### Hosting extensions

Reference the ServiceProviderValidationExtensions.Hosting package and use the `UseServiceProviderExtendedValidation` extension method.

```
Host.CreateDefaultBuilder()
    .UseServiceProviderExtendedValidation()
    .Build();
```

or

```
Host.CreateApplicationBuilder()
    .ConfigureContainerWithServiceProviderExtendedValidation()
    .Build();
```

### Manually building the ServiceProvider

Call `BuildServiceProviderWithValidation` on the `IServiceCollection`, e.g.

```
serviceCollection.BuildServiceProviderWithValidation()
```


