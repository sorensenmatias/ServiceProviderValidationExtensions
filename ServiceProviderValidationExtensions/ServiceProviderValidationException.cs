namespace ServiceProviderValidationExtensions;

public sealed class ServiceProviderValidationException : Exception
{
    public ServiceProviderValidationException(IReadOnlyCollection<string> errors)
    {
        Message =
            $"ServiceProvider validation failed with the following errors:{Environment.NewLine}{Environment.NewLine}{string.Join(Environment.NewLine, errors)}";
    }

    public override string Message { get; }
}