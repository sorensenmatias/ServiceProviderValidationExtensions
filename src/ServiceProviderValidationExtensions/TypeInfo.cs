namespace ServiceProviderValidationExtensions;

public class TypeInfo
{
    public TypeInfo(Type? type)
    {
        Type = type;
        DisplayName = type?.ToString();
    }

    public Type? Type { get; }
    public string? DisplayName { get; }
}
