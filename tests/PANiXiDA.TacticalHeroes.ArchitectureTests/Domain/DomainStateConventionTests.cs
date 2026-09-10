using System.Collections;
using System.Reflection;

using PANiXiDA.Core.Domain;
using PANiXiDA.Core.Domain.AggregateRoots;
using PANiXiDA.Core.Domain.Entities;
using PANiXiDA.Core.Domain.Identifiers;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Domain;

public sealed class DomainStateConventionTests
{
    private const string DomainAssemblySuffix = ".Domain";

    [Fact(DisplayName = "Aggregate roots should contain only value objects identifiers enumerations and entities when state is declared")]
    public void AggregateRoots_Should_ContainOnlyDomainTypes_When_StateIsDeclared()
    {
        var aggregateRoots = GetDomainTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false } &&
                typeof(IAggregateRoot).IsAssignableFrom(type))
            .ToArray();

        Assert.NotEmpty(aggregateRoots);

        var violations = GetStateViolations(
            aggregateRoots,
            allowEntities: true);

        Assert.True(
            violations.Length == 0,
            $"Aggregate root state must contain only value objects, strongly " +
            $"typed identifiers, enumerations, or entities:" +
            $"{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Entities should contain only value objects identifiers and enumerations when state is declared")]
    public void Entities_Should_ContainOnlyDomainTypes_When_StateIsDeclared()
    {
        var entities = GetDomainTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false } &&
                typeof(IEntity).IsAssignableFrom(type) &&
                !typeof(IAggregateRoot).IsAssignableFrom(type))
            .ToArray();

        Assert.NotEmpty(entities);

        var violations = GetStateViolations(
            entities,
            allowEntities: false);

        Assert.True(
            violations.Length == 0,
            $"Entity state must contain only value objects, strongly typed " +
            $"identifiers, or enumerations:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Aggregate roots and entities should accept only domain types when methods are declared")]
    public void AggregateRootsAndEntities_Should_AcceptOnlyDomainTypes_When_MethodsAreDeclared()
    {
        var methods = GetDomainTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } &&
                typeof(IEntity).IsAssignableFrom(type))
            .SelectMany(type => type.GetMethods(
                BindingFlags.Instance |
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly))
            .Where(method => !method.IsPrivate && !method.IsSpecialName)
            .ToArray();
        var violations = methods
            .SelectMany(method => method.GetParameters()
                .Where(parameter =>
                {
                    var types = GetStateTypes(parameter.ParameterType);
                    return types.Length == 0 ||
                           types.Any(type => !IsAllowedDomainType(type, allowEntities: true));
                })
                .Select(parameter =>
                    $"{method.DeclaringType?.FullName}.{method.Name} parameter " +
                    $"'{parameter.Name}' has non-domain type '{parameter.ParameterType}'."))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(methods);
        Assert.True(
            violations.Length == 0,
            $"Aggregate and entity methods must accept only value objects, strongly typed " +
            $"identifiers, enumerations, entities, or collections of these types:" +
            $"{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    private static Type[] GetDomainTypes()
    {
        return
        [
            .. ArchitectureDefinition.ProductionAssemblies
                .Where(assembly => assembly.GetName().Name?.EndsWith(
                    DomainAssemblySuffix,
                    StringComparison.Ordinal) == true)
                .SelectMany(assembly => assembly.GetTypes())
                .OrderBy(type => type.FullName, StringComparer.Ordinal)
        ];
    }

    private static string[] GetStateViolations(
        IEnumerable<Type> domainTypes,
        bool allowEntities)
    {
        return
        [
            .. domainTypes
                .SelectMany(type => type
                    .GetFields(
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.DeclaredOnly)
                    .Select(
                        field => new
                        {
                            DomainType = type,
                            Field = field,
                            StateTypes = GetStateTypes(field.FieldType)
                        }))
                .Where(state => state.StateTypes.Length == 0 ||
                    state.StateTypes.Any(type =>
                        !IsAllowedDomainType(
                            type,
                            allowEntities)))
                .Select(state =>
                    $"{state.DomainType.FullName}.{state.Field.Name} has type " +
                    $"'{state.Field.FieldType}'.")
                .Order(StringComparer.Ordinal)
        ];
    }

    private static Type[] GetStateTypes(Type fieldType)
    {
        var nullableType = Nullable.GetUnderlyingType(fieldType);

        if (nullableType is not null)
        {
            return [nullableType];
        }

        if (!typeof(IEnumerable).IsAssignableFrom(fieldType) ||
            fieldType == typeof(string))
        {
            return [fieldType];
        }

        return
        [
            .. fieldType
                .GetInterfaces()
                .Append(fieldType)
                .Where(type =>
                    type.IsGenericType &&
                    type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .Select(type => type.GetGenericArguments()[0])
                .Distinct()
        ];
    }

    private static bool IsAllowedDomainType(
        Type type,
        bool allowEntities)
    {
        return typeof(ValueObject).IsAssignableFrom(type) ||
               typeof(IStronglyTypedId).IsAssignableFrom(type) ||
               IsEnumeration(type) ||
               allowEntities && typeof(IEntity).IsAssignableFrom(type);
    }

    private static bool IsEnumeration(Type type)
    {
        for (var currentType = type;
             currentType is not null;
             currentType = currentType.BaseType)
        {
            if (currentType.IsGenericType &&
                currentType.GetGenericTypeDefinition() ==
                typeof(Enumeration<>))
            {
                return true;
            }
        }

        return false;
    }
}
