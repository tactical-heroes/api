using System.Reflection;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

using PANiXiDA.Core.Application.Querying.Sorting;
using PANiXiDA.Core.Infrastructure.Persistence.Ef.Read.Sorting;
using PANiXiDA.TacticalHeroes.ArchitectureTests.Global;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Infrastructure;

public sealed class ReadRepositorySortingConventionTests
{
    [Fact(DisplayName = "Read repository methods should apply model sorting when returning collections")]
    public async Task ReadRepositoryMethods_Should_ApplyModelSorting_When_ReturningCollections()
    {
        var methods = await ProductionSourceDocumentDiscovery.GetItemsAsync(GetMethodsAsync);
        var violations = methods.Where(method => !method.AppliesSorting)
            .Select(method => $"{method.Name}: return a collection sorted by IReadModelSorting<{method.Model}> " +
                "using ApplySorting or GetPagedResultAsync.")
            .ToArray();

        Assert.NotEmpty(methods);
        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Read model sorting should include an identifier when default sorting is declared")]
    public void ReadModelSorting_Should_IncludeIdentifier_When_DefaultSortingIsDeclared()
    {
        var sortingTypes = InfrastructurePersistenceConvention.GetConcreteInfrastructureTypes(type =>
            type.GetInterfaces().Any(contract => contract.IsGenericType &&
                contract.GetGenericTypeDefinition() == typeof(IReadModelSorting<>)));
        var violations = sortingTypes.Where(type =>
                type.GetProperty(nameof(IReadModelSorting<object>.DefaultSorting), BindingFlags.Public | BindingFlags.Static)
                    ?.GetValue(null) is not SortingParameters { HasSorting: true } sorting ||
                !sorting.Fields.Any(field => string.Equals(field.Field, "Id", StringComparison.OrdinalIgnoreCase)))
            .Select(type => $"{type.FullName}: DefaultSorting must contain Id to break ties deterministically.")
            .ToArray();

        Assert.NotEmpty(sortingTypes);
        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    private static async Task<CollectionMethod[]> GetMethodsAsync(string repositoryRoot, Document document)
    {
        if (document.Project.AssemblyName?.EndsWith(".Infrastructure", StringComparison.Ordinal) != true)
        {
            return [];
        }

        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        if (root is null || semanticModel is null)
        {
            return [];
        }

        var methods = new List<CollectionMethod>();
        foreach (var declaration in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            if (semanticModel.GetDeclaredSymbol(declaration) is not IMethodSymbol { IsAbstract: false } method ||
                method.IsStatic ||
                (method.DeclaredAccessibility != Accessibility.Public && method.ExplicitInterfaceImplementations.Length == 0) ||
                !method.ContainingType.AllInterfaces.Any(contract => contract.OriginalDefinition.ToDisplayString() ==
                    "PANiXiDA.Core.Application.Persistence.IReadRepository<TId>") ||
                GetCollectionModel(method.ReturnType) is not { } model)
            {
                continue;
            }

            var returns = declaration.ExpressionBody is { } body
                ? [body.Expression]
                : declaration.DescendantNodes(node => node is not AnonymousFunctionExpressionSyntax and not LocalFunctionStatementSyntax)
                    .OfType<ReturnStatementSyntax>().Select(statement => statement.Expression).OfType<ExpressionSyntax>().ToArray();
            methods.Add(new CollectionMethod(
                $"{Path.GetRelativePath(repositoryRoot, document.FilePath!)}:{declaration.GetLocation().GetLineSpan().StartLinePosition.Line + 1}",
                model.ToDisplayString(),
                returns.Length > 0 && returns.All(expression =>
                    UsesSorting(semanticModel.GetOperation(expression), model, declaration, semanticModel))));
        }

        return [.. methods];
    }

    private static ITypeSymbol? GetCollectionModel(ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol array)
        {
            return array.ElementType;
        }

        if (type is not INamedTypeSymbol named || type.SpecialType == SpecialType.System_String)
        {
            return null;
        }

        if ((named.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks" && named.MetadataName is "Task`1" or "ValueTask`1") ||
            (named.ContainingNamespace.ToDisplayString() == "PANiXiDA.Core.ResultPattern" && named.MetadataName == "Result`1"))
        {
            return GetCollectionModel(named.TypeArguments[0]);
        }

        if (named.ContainingNamespace.ToDisplayString() is "PANiXiDA.Core.Application.Querying.Pagination" or
                "PANiXiDA.Core.Application.Querying.Cursor" &&
            named.Name is "PaginationResult" or "CursorPaginationResult")
        {
            return named.TypeArguments[0];
        }

        return named.AllInterfaces.Prepend(named)
            .FirstOrDefault(contract => contract.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T ||
                (contract.MetadataName == "IAsyncEnumerable`1" && contract.ContainingNamespace.ToDisplayString() == "System.Collections.Generic"))
            ?.TypeArguments[0];
    }

    private static bool UsesSorting(IOperation? operation, ITypeSymbol model, MethodDeclarationSyntax declaration, SemanticModel semanticModel)
    {
        switch (operation)
        {
            case IAwaitOperation awaited:
                return UsesSorting(awaited.Operation, model, declaration, semanticModel);
            case IConversionOperation conversion:
                return UsesSorting(conversion.Operand, model, declaration, semanticModel);
            case ILocalReferenceOperation local:
                var value = declaration.DescendantNodes()
                    .Where(node => node.SpanStart < local.Syntax.SpanStart)
                    .Select(node => semanticModel.GetOperation(node))
                    .Select(candidate => candidate switch
                    {
                        IVariableDeclaratorOperation variable when SymbolEqualityComparer.Default.Equals(variable.Symbol, local.Local) => variable.Initializer?.Value,
                        ISimpleAssignmentOperation assignment when assignment.Target is ILocalReferenceOperation target &&
                            SymbolEqualityComparer.Default.Equals(target.Local, local.Local) => assignment.Value,
                        _ => null
                    })
                    .OfType<IOperation>().Where(candidate => candidate.Syntax.Span.End <= local.Syntax.SpanStart)
                    .OrderBy(candidate => candidate.Syntax.SpanStart).LastOrDefault();
                return UsesSorting(value, model, declaration, semanticModel);
            case IInvocationOperation call:
                if (call.TargetMethod.Name == "ApplySorting" && IsSortingFor(call.TargetMethod.ContainingType, model))
                {
                    return true;
                }

                if (call.TargetMethod.Name == "GetPagedResultAsync" &&
                    call.TargetMethod.ContainingType.OriginalDefinition.ToDisplayString() ==
                    "PANiXiDA.Core.Infrastructure.Persistence.Ef.Read.EfReadRepository<TDbContext, TId, TReadDbModel>" &&
                    call.TargetMethod.TypeArguments.Length == 3 && IsSortingFor(call.TargetMethod.TypeArguments[2], model))
                {
                    return true;
                }

                return call.TargetMethod.IsExtensionMethod && call.TargetMethod.Name is
                    "ToListAsync" or "ToArrayAsync" or "ToList" or "ToArray" or "Where" or "Take" or "Skip" or "AsEnumerable" &&
                    UsesSorting(call.Arguments.FirstOrDefault()?.Value, model, declaration, semanticModel);
            default:
                return false;
        }
    }

    private static bool IsSortingFor(ITypeSymbol type, ITypeSymbol model)
    {
        return type.AllInterfaces.Any(contract => contract.OriginalDefinition.ToDisplayString() ==
            "PANiXiDA.Core.Infrastructure.Persistence.Ef.Read.Sorting.IReadModelSorting<TReadModel>" &&
            SymbolEqualityComparer.Default.Equals(contract.TypeArguments[0], model));
    }

    private sealed record CollectionMethod(string Name, string Model, bool AppliesSorting);
}
