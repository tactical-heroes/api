using System.Reflection;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Definitions;

internal static class DomainEventPayloadConvention
{
    internal static IEnumerable<string> GetViolations(Type eventType)
    {
        const BindingFlags Flags = BindingFlags.Instance
            | BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.DeclaredOnly;

        for (var type = eventType; type is not null && type != typeof(object); type = type.BaseType)
        {
            foreach (var property in type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(property => !IsPrimitiveValue(property.PropertyType)))
            {
                yield return $"{type.FullName}.{property.Name} must use a primitive value type, "
                    + $"not '{property.PropertyType}'.";
            }

            foreach (var field in type.GetFields(Flags).Where(field => !IsPrimitiveValue(field.FieldType)))
            {
                yield return $"{type.FullName}.{field.Name} must use a primitive value type, "
                    + $"not '{field.FieldType}'.";
            }
        }
    }

    private static bool IsPrimitiveValue(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        return type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(Guid)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(DateOnly)
            || type == typeof(TimeOnly)
            || type == typeof(TimeSpan);
    }
}
