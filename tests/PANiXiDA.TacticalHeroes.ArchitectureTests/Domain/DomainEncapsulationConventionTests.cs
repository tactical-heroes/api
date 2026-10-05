using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Reflection;

using PANiXiDA.Core.Domain.AggregateRoots;
using PANiXiDA.Core.Domain.DomainEvents;
using PANiXiDA.Core.Domain.Entities;
using PANiXiDA.Core.Domain.Enumerations;
using PANiXiDA.Core.Domain.Identifiers;
using PANiXiDA.Core.Domain.ValueObjects;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Domain;

public sealed class DomainEncapsulationConventionTests
{
    private const string DomainAssemblySuffix = ".Domain";

    [Fact(DisplayName = "Domain events should contain only immutable state when declared")]
    public void DomainEvents_Should_ContainOnlyImmutableState_When_Declared()
    {
        var domainEvents = GetDomainTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false } &&
                typeof(DomainEvent).IsAssignableFrom(type))
            .ToArray();
        var violations = domainEvents
            .SelectMany(DomainImmutableStateConvention.GetViolations)
            .Distinct()
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(domainEvents);
        Assert.True(
            violations.Length == 0,
            $"Domain events must contain only immutable state:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Theory(DisplayName = "Domain events should require immutable collections and elements when collections are declared")]
    [InlineData(typeof(int[]), false)]
    [InlineData(typeof(List<int>), false)]
    [InlineData(typeof(Dictionary<int, string>), false)]
    [InlineData(typeof(HashSet<int>), false)]
    [InlineData(typeof(Queue<int>), false)]
    [InlineData(typeof(Stack<int>), false)]
    [InlineData(typeof(IEnumerable<int>), false)]
    [InlineData(typeof(IReadOnlyCollection<int>), false)]
    [InlineData(typeof(IReadOnlyDictionary<int, string>), false)]
    [InlineData(typeof(IImmutableList<int>), false)]
    [InlineData(typeof(ReadOnlyCollection<int>), false)]
    [InlineData(typeof(ReadOnlyDictionary<int, string>), false)]
    [InlineData(typeof(ReadOnlySet<int>), false)]
    [InlineData(typeof(ImmutableArray<int>.Builder), false)]
    [InlineData(typeof(ImmutableList<int>.Builder), false)]
    [InlineData(typeof(ImmutableDictionary<int, string>.Builder), false)]
    [InlineData(typeof(ImmutableArray<MutableCollectionElement>), false)]
    [InlineData(typeof(ImmutableList<MutableCollectionElement>), false)]
    [InlineData(typeof(ImmutableDictionary<MutableCollectionElement, string>), false)]
    [InlineData(typeof(ImmutableDictionary<string, MutableCollectionElement>), false)]
    [InlineData(typeof(FrozenSet<MutableCollectionElement>), false)]
    [InlineData(typeof(FrozenDictionary<string, MutableCollectionElement>), false)]
    [InlineData(typeof(ImmutableArray<ImmutableList<MutableCollectionElement>>), false)]
    [InlineData(typeof(ImmutableArray<object>), false)]
    [InlineData(typeof(ImmutableArray<List<int>>), false)]
    [InlineData(typeof(Memory<int>), false)]
    [InlineData(typeof(ReadOnlyMemory<int>), false)]
    [InlineData(typeof(ImmutableArray<int>), true)]
    [InlineData(typeof(ImmutableList<int>), true)]
    [InlineData(typeof(ImmutableHashSet<int>), true)]
    [InlineData(typeof(ImmutableSortedSet<int>), true)]
    [InlineData(typeof(ImmutableDictionary<int, string>), true)]
    [InlineData(typeof(ImmutableSortedDictionary<int, string>), true)]
    [InlineData(typeof(ImmutableQueue<int>), true)]
    [InlineData(typeof(ImmutableStack<int>), true)]
    [InlineData(typeof(FrozenSet<int>), true)]
    [InlineData(typeof(FrozenDictionary<int, string>), true)]
    [InlineData(typeof(ImmutableArray<ImmutableCollectionElement>), true)]
    [InlineData(typeof(ImmutableArray<ImmutableArray<int>>), true)]
    [InlineData(typeof(ImmutableDictionary<string, ImmutableArray<ImmutableCollectionElement>>), true)]
    public void DomainEvents_Should_RequireImmutableCollectionsAndElements_When_CollectionsAreDeclared(
        Type collectionType,
        bool expected)
    {
        var eventType = typeof(CollectionDomainEvent<>).MakeGenericType(collectionType);

        var violations = DomainImmutableStateConvention.GetViolations(eventType).ToArray();

        Assert.Equal(expected, violations.Length == 0);
    }

    [Fact(DisplayName = "Value objects should contain only immutable state when declared")]
    public void ValueObjects_Should_ContainOnlyImmutableState_When_Declared()
    {
        var valueObjects = GetDomainTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false } &&
                typeof(ValueObject).IsAssignableFrom(type))
            .ToArray();
        var violations = valueObjects
            .SelectMany(DomainImmutableStateConvention.GetViolations)
            .Distinct()
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(valueObjects);
        Assert.True(
            violations.Length == 0,
            $"Value objects must contain only immutable state:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Strongly typed ids should contain only immutable state when declared")]
    public void StronglyTypedIds_Should_ContainOnlyImmutableState_When_Declared()
    {
        var identifiers = GetStronglyTypedIds();
        var violations = identifiers
            .SelectMany(DomainImmutableStateConvention.GetViolations)
            .Distinct()
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(identifiers);
        Assert.True(
            violations.Length == 0,
            $"Strongly typed ids must contain only immutable state:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Enumerations should contain only immutable state when declared")]
    public void Enumerations_Should_ContainOnlyImmutableState_When_Declared()
    {
        var enumerations = GetDomainTypes()
            .Where(type => !type.IsAbstract &&
                           InfrastructurePersistenceConvention.GetClosedGenericBaseType(
                               type, typeof(Enumeration<>)) is not null)
            .ToArray();
        var violations = enumerations
            .SelectMany(DomainImmutableStateConvention.GetViolations)
            .Distinct()
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(enumerations);
        Assert.True(
            violations.Length == 0,
            $"Enumerations must contain only immutable state:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Value objects and enumerations should declare public getters without setters when properties are declared")]
    public void ValueObjectsAndEnumerations_Should_DeclarePublicGettersWithoutSetters_When_PropertiesAreDeclared()
    {
        var domainTypes = GetDomainTypes().ToArray();
        var valueObjects = domainTypes
            .Where(type => typeof(ValueObject).IsAssignableFrom(type))
            .ToArray();
        var enumerations = domainTypes
            .Where(type => InfrastructurePersistenceConvention.GetClosedGenericBaseType(
                type,
                typeof(Enumeration<>)) is not null)
            .ToArray();
        var violations = valueObjects.Concat(enumerations)
            .SelectMany(type => type
                .GetProperties(
                    BindingFlags.Instance |
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.NonPublic)
                .Where(property =>
                    property.GetGetMethod(nonPublic: true) is not { IsPublic: true } ||
                    property.GetSetMethod(nonPublic: true) is not null)
                .Select(property =>
                    $"{type.FullName}.{property.Name} must have a public getter " +
                    $"and no setter or init accessor."))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(valueObjects);
        Assert.NotEmpty(enumerations);
        Assert.True(
            violations.Length == 0,
            $"Value object and enumeration property violations:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Aggregate roots and entities should not expose writable state when domain state is declared")]
    public void AggregateRootsAndEntities_Should_NotExposeWritableState_When_DomainStateIsDeclared()
    {
        var domainEntities = GetDomainEntities();
        var violations = domainEntities
            .SelectMany(GetExternalMutationViolations)
            .Distinct()
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(domainEntities);
        Assert.True(
            violations.Length == 0,
            $"Externally writable domain state:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Theory(DisplayName = "Domain state should reject external writes when member access varies")]
    [InlineData(typeof(PublicSetterState), false)]
    [InlineData(typeof(InternalSetterState), false)]
    [InlineData(typeof(PublicInitState), false)]
    [InlineData(typeof(ExplicitSetterState), false)]
    [InlineData(typeof(PublicFieldState), false)]
    [InlineData(typeof(InternalFieldState), false)]
    [InlineData(typeof(WritableReferenceState), false)]
    [InlineData(typeof(InheritedSetterState), false)]
    [InlineData(typeof(PrivateSetterState), true)]
    [InlineData(typeof(ProtectedSetterState), true)]
    [InlineData(typeof(ReadOnlyFieldState), true)]
    [InlineData(typeof(ReadOnlyReferenceState), true)]
    public void DomainState_Should_RejectExternalWrites_When_MemberAccessVaries(Type type, bool expected)
    {
        var violations = GetExternalMutationViolations(type).ToArray();

        Assert.Equal(expected, violations.Length == 0);
    }

    [Theory(DisplayName = "Immutable state should validate fields recursively when state shape varies")]
    [InlineData(typeof(PublicSetterState), false)]
    [InlineData(typeof(PrivateSetterState), false)]
    [InlineData(typeof(PublicFieldState), false)]
    [InlineData(typeof(ReadOnlyMutableFieldState), false)]
    [InlineData(typeof(InheritedPrivateFieldState), false)]
    [InlineData(typeof(MutableStructState), false)]
    [InlineData(typeof(RecursiveMutableState), false)]
    [InlineData(typeof(BorrowedReferenceState), false)]
    [InlineData(typeof(ReadOnlyFieldState), true)]
    [InlineData(typeof(PublicInitState), true)]
    [InlineData(typeof(ReadOnlyStructState), true)]
    [InlineData(typeof(ReadOnlyReferenceState), true)]
    [InlineData(typeof(RecursiveImmutableState), true)]
    [InlineData(typeof(int?), true)]
    public void ImmutableState_Should_ValidateFieldsRecursively_When_StateShapeVaries(Type type, bool expected)
    {
        var violations = DomainImmutableStateConvention.GetViolations(type).ToArray();

        Assert.Equal(expected, violations.Length == 0);
    }

    [Fact(DisplayName = "Aggregate roots and entities should not expose mutable collections when public state is declared")]
    public void AggregateRootsAndEntities_Should_NotExposeMutableCollections_When_PublicStateIsDeclared()
    {
        var domainEntities = GetDomainEntities();
        var violations = domainEntities
            .SelectMany(GetPublicStateMembers)
            .Where(member =>
                ContainsMutableCollection(member.MemberType))
            .Select(member =>
                $"{member.DeclaringType.FullName}.{member.Name} exposes " +
                $"mutable collection type '{member.MemberType}'.")
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(domainEntities);
        Assert.True(
            violations.Length == 0,
            $"Mutable collections exposed by domain types:" +
            $"{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Aggregate roots should not contain other aggregate roots when state is declared")]
    public void AggregateRoots_Should_NotContainOtherAggregateRoots_When_StateIsDeclared()
    {
        var aggregateRoots = GetDomainEntities()
            .Where(type =>
                typeof(IAggregateRoot).IsAssignableFrom(type))
            .ToArray();
        var violations = aggregateRoots
            .SelectMany(type => type
                .GetFields(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly)
                .Where(field => GetContainedTypes(field.FieldType)
                    .Any(containedType =>
                        typeof(IAggregateRoot).IsAssignableFrom(
                            containedType)))
                .Select(field =>
                    $"{type.FullName}.{field.Name} must not contain " +
                    $"aggregate root type '{field.FieldType}'."))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(aggregateRoots);
        Assert.True(
            violations.Length == 0,
            $"Nested aggregate roots:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Domain objects should declare only private constructors when created through factories")]
    public void DomainObjects_Should_DeclareOnlyPrivateConstructors_When_CreatedThroughFactories()
    {
        var domainObjects = GetDomainTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false } &&
                (typeof(IEntity).IsAssignableFrom(type) ||
                 typeof(ValueObject).IsAssignableFrom(type) ||
                 InfrastructurePersistenceConvention.GetClosedGenericBaseType(
                     type,
                     typeof(Enumeration<>)) is not null))
            .ToArray();
        var violations = domainObjects
            .SelectMany(type => type
                .GetConstructors(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly)
                .Where(constructor => !constructor.IsPrivate)
                .Select(constructor =>
                    $"{type.FullName} declares non-private constructor " +
                    $"'{constructor}' and must expose creation through a " +
                    $"factory method instead."))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(domainObjects);
        Assert.True(
            violations.Length == 0,
            $"Non-private Domain object constructors:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Strongly typed ids should declare only private constructors when declared")]
    public void StronglyTypedIds_Should_DeclareOnlyPrivateConstructors_When_Declared()
    {
        var identifiers = GetStronglyTypedIds();
        var violations = identifiers
            .SelectMany(type => type
                .GetConstructors(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly)
                .Where(constructor => !constructor.IsPrivate)
                .Select(constructor =>
                    $"{type.FullName} declares non-private constructor " +
                    $"'{constructor}' and must expose creation through a " +
                    $"factory method instead."))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(identifiers);
        Assert.True(
            violations.Length == 0,
            $"Non-private strongly typed id constructors:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Strongly typed ids should declare public getters without setters when properties are declared")]
    public void StronglyTypedIds_Should_DeclarePublicGettersWithoutSetters_When_PropertiesAreDeclared()
    {
        var identifiers = GetStronglyTypedIds();
        var violations = identifiers
            .SelectMany(type => type
                .GetProperties(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly)
                .Where(property =>
                    property.GetGetMethod(nonPublic: true) is not { IsPublic: true } ||
                    property.GetSetMethod(nonPublic: true) is not null)
                .Select(property =>
                    $"{type.FullName}.{property.Name} must have a public getter " +
                    $"and no setter or init accessor."))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(identifiers);
        Assert.True(
            violations.Length == 0,
            $"Strongly typed id property violations:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    private static IEnumerable<string> GetExternalMutationViolations(Type type)
    {
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public |
                                   BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        for (var currentType = type;
             currentType is not null && currentType != typeof(object);
             currentType = currentType.BaseType)
        {
            foreach (var field in currentType.GetFields(Flags).Where(field =>
                         !field.IsInitOnly && (field.IsPublic || field.IsAssembly || field.IsFamilyOrAssembly)))
            {
                yield return $"{currentType.FullName}.{field.Name} must not expose a writable field.";
            }

            foreach (var violation in currentType.GetMethods(Flags)
                         .Where(IsExternallyAccessible)
                         .SelectMany(GetExternalMethodMutationViolations))
            {
                yield return violation;
            }
        }
    }

    private static IEnumerable<string> GetExternalMethodMutationViolations(MethodInfo method)
    {
        if (method.IsSpecialName &&
            (method.Name.StartsWith("set_", StringComparison.Ordinal) ||
             method.Name.Contains(".set_", StringComparison.Ordinal)))
        {
            yield return $"{method.DeclaringType?.FullName}.{method.Name} must not expose a setter or init accessor.";
        }

        if (DomainImmutableStateConvention.ReturnsWritableReference(method))
        {
            yield return $"{method.DeclaringType?.FullName}.{method.Name} must not return a writable reference.";
        }
    }

    private static bool IsExternallyAccessible(MethodInfo method)
    {
        return method.IsPublic || method.IsAssembly || method.IsFamilyOrAssembly ||
               method is { IsPrivate: true, IsVirtual: true, IsFinal: true };
    }

    private static Type[] GetStronglyTypedIds()
    {
        return
        [
            .. GetDomainTypes()
                .Where(type =>
                    (type.IsClass || type.IsValueType) &&
                    typeof(IStronglyTypedId).IsAssignableFrom(type))
        ];
    }

    private static Type[] GetDomainEntities()
    {
        return
        [
            .. GetDomainTypes()
                .Where(type =>
                    type is { IsClass: true, IsAbstract: false } &&
                    typeof(IEntity).IsAssignableFrom(type))
                .OrderBy(type => type.FullName, StringComparer.Ordinal)
        ];
    }

    private static IEnumerable<Type> GetDomainTypes()
    {
        return ArchitectureDefinition.s_productionAssemblies
            .Where(assembly => assembly.GetName().Name?.EndsWith(
                DomainAssemblySuffix,
                StringComparison.Ordinal) == true)
            .SelectMany(assembly => assembly.GetTypes());
    }

    private static IEnumerable<PublicStateMember> GetPublicStateMembers(
        Type domainType)
    {
        var properties = domainType
            .GetProperties(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.DeclaredOnly)
            .Where(property =>
                property.GetMethod is { IsPublic: true })
            .Select(property => new PublicStateMember(
                domainType,
                property.Name,
                property.PropertyType));
        var fields = domainType
            .GetFields(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.DeclaredOnly)
            .Select(field => new PublicStateMember(
                domainType,
                field.Name,
                field.FieldType));
        var methods = domainType
            .GetMethods(
                BindingFlags.Instance |
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.DeclaredOnly)
            .Where(method =>
                !method.IsSpecialName &&
                method.ReturnType != typeof(void))
            .Select(method => new PublicStateMember(
                domainType,
                method.Name + "()",
                method.ReturnType));

        return properties
            .Concat(fields)
            .Concat(methods);
    }

    private static bool ContainsMutableCollection(Type type)
    {
        if (type == typeof(string))
        {
            return false;
        }

        if (DomainCollectionConvention.IsCollectionType(type) &&
            !DomainCollectionConvention.IsReadOnlyCollectionType(type))
        {
            return true;
        }

        return type.IsGenericType &&
               type.GetGenericArguments()
                   .Any(ContainsMutableCollection);
    }

    private static IEnumerable<Type> GetContainedTypes(Type type)
    {
        yield return type;

        var nullableType = Nullable.GetUnderlyingType(type);

        if (nullableType is not null)
        {
            foreach (var containedType in GetContainedTypes(nullableType))
            {
                yield return containedType;
            }
        }

        if (type.IsArray && type.GetElementType() is { } elementType)
        {
            foreach (var containedType in GetContainedTypes(elementType))
            {
                yield return containedType;
            }
        }

        if (!type.IsGenericType)
        {
            yield break;
        }

        foreach (var genericArgument in type.GetGenericArguments())
        {
            foreach (var containedType in GetContainedTypes(genericArgument))
            {
                yield return containedType;
            }
        }
    }

    private static class MutableStaticState
    {
        public static readonly int[] Values = [1, 2];
    }

    private sealed record CollectionDomainEvent<T>(T Items) : DomainEvent;

    private interface IWritableState
    {
        int Value { get; set; }
    }

    private sealed class PublicSetterState
    {
        public int Value { get; set; }
    }

    private sealed class InternalSetterState
    {
        public int Value { get; internal set; }
    }

    private sealed class PublicInitState
    {
        public int Value { get; init; }
    }

    private sealed class ExplicitSetterState : IWritableState
    {
        int IWritableState.Value { get; set; }
    }

    private sealed class PublicFieldState
    {
        public int Value = 1;
    }

    private sealed class InternalFieldState
    {
        internal int _value = 1;
    }

    private sealed class WritableReferenceState
    {
        private int _value = 1;

        public ref int Value => ref _value;
    }

    private abstract class SetterStateBase
    {
        public int Value { get; set; }
    }

    private sealed class InheritedSetterState : SetterStateBase;

    private sealed class PrivateSetterState
    {
        public int Value { get; private set; }
    }

    private class ProtectedSetterState
    {
        public int Value { get; protected set; }
    }

    private sealed class ReadOnlyFieldState
    {
        public readonly int Value = 1;
    }

    private sealed class ReadOnlyReferenceState
    {
        private readonly int _value = 1;

        public ref readonly int Value => ref _value;
    }

    private sealed class BorrowedReferenceState
    {
        private readonly int _index = 1;

        public ref int Value => ref MutableStaticState.Values[_index];
    }

    private sealed class ReadOnlyMutableFieldState
    {
        public readonly MutableCollectionElement Value = new();
    }

    private abstract class PrivateFieldStateBase
    {
        private int _value;

        public int Value => ++_value;
    }

    private sealed class InheritedPrivateFieldState : PrivateFieldStateBase;

    private struct MutableStructState
    {
        public int Value { get; set; }
    }

    private readonly record struct ReadOnlyStructState(int Value);

    private sealed record RecursiveImmutableState
    {
        public RecursiveImmutableState? Next { get; init; }
    }

    private sealed record RecursiveMutableState
    {
        public RecursiveMutableState? Next { get; init; }
        public int Value { get; set; }
    }

    private sealed record ImmutableCollectionElement(int Value);

    private sealed class MutableCollectionElement
    {
        public int Value { get; set; }
    }

    private sealed record PublicStateMember(
        Type DeclaringType,
        string Name,
        Type MemberType);
}
