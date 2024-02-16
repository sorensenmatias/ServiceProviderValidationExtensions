namespace ServiceProviderValidationExtensions.Internal;

internal class ValidationRegistrations
{
    public ServicesRegistrations Services = new ServicesRegistrations();
    
    public ImplementationRegistrations Implementations = new ImplementationRegistrations();

    internal class ServicesRegistrations
    {
        private readonly List<Type> _exclusives = new();

        public IReadOnlyCollection<Type> Exclusives => _exclusives;

        public void RegisterExclusive<TService>(ServiceValidation serviceValidation)
        {
            switch (serviceValidation)
            {
                case ServiceValidation.None:
                    break;
                case ServiceValidation.ExclusiveService:
                    _exclusives.Add(typeof(TService));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(serviceValidation), serviceValidation, null);
            }
        }
    }

    internal class ImplementationRegistrations
    {
        private readonly List<(Type service, Type implementation)> _exclusives = new();

        public IReadOnlyCollection<(Type service, Type implementation)> Exclusives => _exclusives;

        public void RegisterExclusive<TService, TImplementation>(ImplementationValidation implementationValidation)
        {
            switch (implementationValidation)
            {
                case ImplementationValidation.None:
                    break;
                case ImplementationValidation.ExclusiveImplementation:
                    _exclusives.Add((typeof(TService), typeof(TImplementation)));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(implementationValidation), implementationValidation, null);
            }
        }
    }
}

