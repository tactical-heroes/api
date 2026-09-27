using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Definitions;

internal static class DomainImmutableStateConvention
{
    internal static IEnumerable<string> GetViolations(Type type)
    {
        return GetViolations(type, new HashSet<Type>());
    }

    internal static bool ReturnsWritableReference(MethodInfo method)
    {
        return method.ReturnType.IsByRef &&
               !method.ReturnParameter.GetRequiredCustomModifiers().Contains(typeof(InAttribute));
    }

    private static IEnumerable<string> GetViolations(Type type, ISet<Type> visitedTypes)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        if (IsImmutableScalar(type) || !visitedTypes.Add(type))
        {
            yield break;
        }

        if (DomainCollectionConvention.IsImmutableCollectionType(type))
        {
            foreach (var argument in type.GetGenericArguments())
            {
                foreach (var violation in GetViolations(argument, visitedTypes))
                {
                    yield return $"{type}: {violation}";
                }
            }

            yield break;
        }

        if (DomainCollectionConvention.IsCollectionType(type))
        {
            yield return $"Collection type '{type}' must be a standard immutable " +
                         "or frozen collection; mutable collections, read-only " +
                         "wrappers and collection interfaces are not allowed.";
            yield break;
        }

        if (!type.IsValueType && !type.IsSealed)
        {
            yield return $"State type '{type}' must be a scalar, value type, " +
                         "or sealed class with immutable state; polymorphic " +
                         "state is not allowed.";
            yield break;
        }

        for (var currentType = type;
             currentType is not null && currentType != typeof(object);
             currentType = currentType.BaseType)
        {
            foreach (var violation in GetDeclaredViolations(currentType, visitedTypes))
            {
                yield return violation;
            }
        }
    }

    private static IEnumerable<string> GetDeclaredViolations(Type type, ISet<Type> visitedTypes)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public |
                                   BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        foreach (var property in type.GetProperties(flags))
        {
            var setter = property.GetSetMethod(nonPublic: true);

            if (setter is not null &&
                !setter.ReturnParameter.GetRequiredCustomModifiers().Contains(typeof(IsExternalInit)))
            {
                yield return $"{type.FullName}.{property.Name} must not " +
                             "declare a setter; only get or init is allowed.";
            }
        }

        foreach (var method in type.GetMethods(flags).Where(ReturnsWritableReference))
        {
            yield return $"{type.FullName}.{method.Name} must not return a writable reference.";
        }

        foreach (var field in type.GetFields(flags))
        {
            if (!field.IsInitOnly)
            {
                yield return $"{type.FullName}.{field.Name} must be readonly.";
            }

            foreach (var violation in GetViolations(field.FieldType, visitedTypes))
            {
                yield return $"{type.FullName}.{field.Name}: {violation}";
            }
        }
    }

    private static bool IsImmutableScalar(Type type)
    {
        return type.IsPrimitive || type.IsEnum ||
               type == typeof(string) || type == typeof(decimal) || type == typeof(Guid) ||
               type == typeof(DateTime) || type == typeof(DateTimeOffset) ||
               type == typeof(DateOnly) || type == typeof(TimeOnly) || type == typeof(TimeSpan);
    }
}
