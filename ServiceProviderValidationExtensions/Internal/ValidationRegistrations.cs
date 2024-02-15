namespace ServiceProviderValidationExtensions.Internal;

internal class ValidationRegistrations
{
    private readonly List<Type> _exclusives = new();

    public IReadOnlyCollection<Type> Exclusives => _exclusives;

    public void RegisterExclusive<TService>()
    {
        _exclusives.Add(typeof(TService));
    }
}