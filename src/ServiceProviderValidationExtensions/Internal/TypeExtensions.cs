namespace ServiceProviderValidationExtensions.Internal;

internal static class TypeExtensions
{
    public static bool IsDerivedFromGenericParent(this Type? type, Type parentType)
    {
        if (!parentType.IsGenericType)
        {
            throw new ArgumentException("type must be generic", nameof(parentType));
        }

        if (type == null || type == typeof(object))
        {
            return false;
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == parentType)
        {
            return true;
        }

        if (type.BaseType.IsDerivedFromGenericParent(parentType))
        {
            return true;
        }

        if (type.GetInterfaces().Any(t => t.IsDerivedFromGenericParent(parentType)))
        {
            return true;
        }

        return false;
    }
}
