using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Definitions;

internal static class DomainCollectionConvention
{
    private static readonly Type[] CollectionViewTypes =
    [
        typeof(Span<>),
        typeof(ReadOnlySpan<>),
        typeof(Memory<>),
        typeof(ReadOnlyMemory<>)
    ];

    private static readonly Type[] ImmutableCollectionTypes =
    [
        typeof(ImmutableArray<>),
        typeof(ImmutableList<>),
        typeof(ImmutableHashSet<>),
        typeof(ImmutableSortedSet<>),
        typeof(ImmutableDictionary<,>),
        typeof(ImmutableSortedDictionary<,>),
        typeof(ImmutableQueue<>),
        typeof(ImmutableStack<>),
        typeof(FrozenSet<>),
        typeof(FrozenDictionary<,>)
    ];

    private static readonly Type[] ProtectedCollectionTypes =
    [
        typeof(ReadOnlyCollection<>),
        typeof(ReadOnlyDictionary<,>),
        typeof(ReadOnlySet<>),
        typeof(ReadOnlySpan<>),
        typeof(ReadOnlyMemory<>),
        .. ImmutableCollectionTypes
    ];

    private static readonly Type[] ReadOnlyCollectionInterfaces =
    [
        typeof(IEnumerable<>),
        typeof(IReadOnlyCollection<>),
        typeof(IReadOnlyList<>),
        typeof(IReadOnlyDictionary<,>),
        typeof(IReadOnlySet<>)
    ];

    internal static bool IsImmutableCollectionType(Type type)
    {
        return type.IsGenericType &&
               ImmutableCollectionTypes.Contains(type.GetGenericTypeDefinition());
    }

    internal static bool IsReadOnlyCollectionType(Type type)
    {
        return type == typeof(System.Collections.IEnumerable) ||
               type.IsGenericType &&
               (ProtectedCollectionTypes.Contains(type.GetGenericTypeDefinition()) ||
                ReadOnlyCollectionInterfaces.Contains(type.GetGenericTypeDefinition()));
    }

    internal static bool IsCollectionType(Type type)
    {
        return type != typeof(string) &&
               (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) ||
                type.IsGenericType && CollectionViewTypes.Contains(type.GetGenericTypeDefinition()));
    }

    internal static bool IsCollection(ITypeSymbol type, Compilation compilation)
    {
        var enumerable = compilation.GetTypeByMetadataName("System.Collections.IEnumerable");

        return type.SpecialType != SpecialType.System_String &&
               (SymbolEqualityComparer.Default.Equals(type, enumerable) ||
                type.AllInterfaces.Any(candidate =>
                    SymbolEqualityComparer.Default.Equals(candidate, enumerable)) ||
                type is INamedTypeSymbol namedType && CollectionViewTypes.Any(candidate =>
                    SymbolEqualityComparer.Default.Equals(
                        namedType.OriginalDefinition,
                        compilation.GetTypeByMetadataName(candidate.FullName!))));
    }

    internal static bool IsProtectedCollection(IOperation? operation, Compilation compilation)
    {
        if (operation is { ConstantValue: { HasValue: true, Value: null } })
        {
            return true;
        }

        return operation switch
        {
            IThrowOperation => true,
            IConversionOperation conversion => IsProtectedConversion(conversion, compilation),
            IParenthesizedOperation parenthesized => IsProtectedCollection(parenthesized.Operand, compilation),
            IConditionalOperation conditional =>
                IsProtectedCollection(conditional.WhenTrue, compilation) &&
                IsProtectedCollection(conditional.WhenFalse, compilation),
            ICoalesceOperation coalesce =>
                IsProtectedCollection(coalesce.Value, compilation) &&
                IsProtectedCollection(coalesce.WhenNull, compilation),
            ISwitchExpressionOperation switchExpression => switchExpression.Arms.All(arm =>
                IsProtectedCollection(arm.Value, compilation)),
            ITupleOperation tuple => tuple.Elements.All(element =>
                !ReturnsCollection(element, compilation) || IsProtectedCollection(element, compilation)),
            { Type: { } type } => IsProtectedCollectionType(type, compilation),
            _ => false
        };
    }

    internal static bool IsProtectedCollectionType(ITypeSymbol type, Compilation compilation)
    {
        if (type is not INamedTypeSymbol namedType)
        {
            return false;
        }

        var isCollection = IsCollection(type, compilation);
        var collectionArguments = namedType.TypeArguments
            .Where(argument => ContainsCollection(argument, compilation))
            .ToArray();

        return (isCollection || collectionArguments.Length > 0) &&
               (!isCollection || ProtectedCollectionTypes.Any(candidate =>
                   SymbolEqualityComparer.Default.Equals(
                       namedType.OriginalDefinition,
                       compilation.GetTypeByMetadataName(candidate.FullName!)))) &&
               collectionArguments.All(argument => IsProtectedCollectionType(argument, compilation));
    }

    internal static bool ContainsCollection(ITypeSymbol type, Compilation compilation)
    {
        return IsCollection(type, compilation) ||
               type is INamedTypeSymbol namedType && namedType.TypeArguments.Any(argument =>
                   ContainsCollection(argument, compilation));
    }

    internal static bool ReturnsCollection(IOperation? operation, Compilation compilation)
    {
        return operation switch
        {
            IConversionOperation conversion => ReturnsCollection(conversion.Operand, compilation),
            IParenthesizedOperation parenthesized => ReturnsCollection(parenthesized.Operand, compilation),
            IConditionalOperation conditional => ReturnsCollection(conditional.WhenTrue, compilation) ||
                                                 ReturnsCollection(conditional.WhenFalse, compilation),
            ICoalesceOperation coalesce => ReturnsCollection(coalesce.Value, compilation) ||
                                           ReturnsCollection(coalesce.WhenNull, compilation),
            ISwitchExpressionOperation switchExpression => switchExpression.Arms.Any(arm =>
                ReturnsCollection(arm.Value, compilation)),
            ITupleOperation tuple => tuple.Elements.Any(element => ReturnsCollection(element, compilation)),
            { Type: { } type } => ContainsCollection(type, compilation),
            _ => false
        };
    }

    private static bool IsProtectedConversion(IConversionOperation conversion, Compilation compilation)
    {
        if (conversion.Type is { } type && IsReadOnlyViewType(type, compilation) &&
            IsProtectedCollectionType(type, compilation) &&
            (conversion.OperatorMethod is null || CollectionViewTypes.Any(candidate =>
                SymbolEqualityComparer.Default.Equals(
                    conversion.OperatorMethod.ContainingType.OriginalDefinition,
                    compilation.GetTypeByMetadataName(candidate.FullName!)))))
        {
            return true;
        }

        return conversion.OperatorMethod is null && IsProtectedCollection(conversion.Operand, compilation);
    }

    private static bool IsReadOnlyViewType(ITypeSymbol type, Compilation compilation)
    {
        return type is INamedTypeSymbol namedType &&
               (SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition,
                    compilation.GetTypeByMetadataName(typeof(ReadOnlySpan<>).FullName!)) ||
                SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition,
                    compilation.GetTypeByMetadataName(typeof(ReadOnlyMemory<>).FullName!)));
    }
}
