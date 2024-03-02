# ServiceProviderValidationExtensions

This library makes more fine-grained control possible for registration in the ServiceProvider.
These are especially useful for large complicated application.

It contains the features listed below:

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

## Invoking the validation

There are two ways of invoking the validation:

### Hosting extensions

Reference the ServiceProviderValidationExtensions.Hosting package and use the `UseServiceProviderExtendedValidation` extension method.

```
Host.CreateDefaultBuilder()
    .UseServiceProviderExtendedValidation()
```

or

```
Host.CreateApplicationBuilder()
    .ConfigureContainerWithServiceProviderExtendedValidation()
```

### Manually building the ServiceProvider

Call `BuildServiceProviderWithValidation` on the `IServiceCollection`, e.g.

```
serviceCollection.BuildServiceProviderWithValidation()
```


