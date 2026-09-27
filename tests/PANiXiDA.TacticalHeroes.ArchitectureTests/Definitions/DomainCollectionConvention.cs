using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Definitions;

internal static class DomainCollectionConvention
{
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

    internal static bool IsCollection(ITypeSymbol type, Compilation compilation)
    {
        var enumerable = compilation.GetTypeByMetadataName("System.Collections.IEnumerable");

        return type.SpecialType != SpecialType.System_String &&
               (SymbolEqualityComparer.Default.Equals(type, enumerable) ||
                type.AllInterfaces.Any(candidate =>
                    SymbolEqualityComparer.Default.Equals(candidate, enumerable)));
    }

    internal static bool IsProtectedCollection(IOperation? operation, Compilation compilation)
    {
        return operation switch
        {
            IConversionOperation conversion => conversion.OperatorMethod is null &&
                                               IsProtectedCollection(conversion.Operand, compilation),
            IParenthesizedOperation parenthesized => IsProtectedCollection(parenthesized.Operand, compilation),
            IConditionalOperation conditional =>
                IsProtectedCollection(conditional.WhenTrue, compilation) &&
                IsProtectedCollection(conditional.WhenFalse, compilation),
            ICoalesceOperation coalesce =>
                IsProtectedCollection(coalesce.Value, compilation) &&
                IsProtectedCollection(coalesce.WhenNull, compilation),
            ISwitchExpressionOperation switchExpression => switchExpression.Arms.All(arm =>
                IsProtectedCollection(arm.Value, compilation)),
            { Type: { } type } => IsProtectedCollectionType(type, compilation),
            _ => false
        };
    }

    internal static bool IsProtectedCollectionType(ITypeSymbol type, Compilation compilation)
    {
        return type is INamedTypeSymbol namedType && ProtectedCollectionTypes.Any(candidate =>
            SymbolEqualityComparer.Default.Equals(
                namedType.OriginalDefinition,
                compilation.GetTypeByMetadataName(candidate.FullName!)));
    }
}
