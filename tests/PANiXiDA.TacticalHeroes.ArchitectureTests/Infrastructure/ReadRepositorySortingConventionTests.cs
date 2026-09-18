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
        var methods = (await ProductionSourceDocumentDiscovery.GetItemsAsync(GetMethodsAsync))
            .Where(method => method.IsCollection).ToArray();
        var violations = methods.Where(method => !method.AppliesSorting)
            .Select(method => $"{method.Name}: return a collection sorted by IReadModelSorting<{method.Model}> " +
                "using ApplySorting or GetPagedResultAsync.")
            .ToArray();

        Assert.NotEmpty(methods);
        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "EF read repository methods should apply matching mappers when returning read models")]
    public async Task EfReadRepositoryMethods_Should_ApplyMatchingMappers_When_ReturningReadModels()
    {
        var methods = (await ProductionSourceDocumentDiscovery.GetItemsAsync(GetMethodsAsync))
            .Where(method => method.IsEfRepository).ToArray();
        var violations = methods.Where(method => !method.AppliesMapping)
            .Select(method => $"{method.Name}: return {method.Model} using IReadModelMapper<TId, TReadDbModel, TReadModel> " +
                "matching the repository through ProjectTo, GetByIdAsync, or GetPagedResultAsync.")
            .ToArray();

        Assert.NotEmpty(methods);
        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Read model sorting should be nonempty when default sorting is declared")]
    public void ReadModelSorting_Should_BeNonempty_When_DefaultSortingIsDeclared()
    {
        var sortingTypes = InfrastructurePersistenceConvention.GetConcreteInfrastructureTypes(type =>
            type.GetInterfaces().Any(contract => contract.IsGenericType &&
                contract.GetGenericTypeDefinition() == typeof(IReadModelSorting<>)));
        var violations = sortingTypes.Where(type =>
                type.GetProperty(nameof(IReadModelSorting<>.DefaultSorting), BindingFlags.Public | BindingFlags.Static)
                    ?.GetValue(null) is not SortingParameters { HasSorting: true })
            .Select(type => $"{type.FullName}: DefaultSorting must contain at least one sorting field.")
            .ToArray();

        Assert.NotEmpty(sortingTypes);
        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    private static async Task<ReadMethod[]> GetMethodsAsync(string repositoryRoot, Document document)
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

        var methods = new List<ReadMethod>();
        foreach (var declaration in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            if (semanticModel.GetDeclaredSymbol(declaration) is not IMethodSymbol { IsAbstract: false } method ||
                method.IsStatic ||
                (method.DeclaredAccessibility != Accessibility.Public && method.ExplicitInterfaceImplementations.Length == 0) ||
                !method.ContainingType.AllInterfaces.Any(contract => contract.OriginalDefinition.ToDisplayString() ==
                    "PANiXiDA.Core.Application.Persistence.IReadRepository<TId>") ||
                GetResultModel(method.ReturnType, collectionsOnly: false) is not { } model)
            {
                continue;
            }

            var returns = declaration.ExpressionBody is { } body
                ? [body.Expression]
                : declaration.DescendantNodes(node => node is not AnonymousFunctionExpressionSyntax and not LocalFunctionStatementSyntax)
                    .OfType<ReturnStatementSyntax>().Select(statement => statement.Expression).OfType<ExpressionSyntax>().ToArray();
            var repository = method.ContainingType;
            while (repository is not null && !IsEfRepository(repository))
            {
                repository = repository.BaseType;
            }

            methods.Add(new ReadMethod(
                $"{Path.GetRelativePath(repositoryRoot, document.FilePath!)}:{declaration.GetLocation().GetLineSpan().StartLinePosition.Line + 1}",
                model.ToDisplayString(),
                GetResultModel(method.ReturnType, collectionsOnly: true) is not null,
                repository is not null,
                returns.Length > 0 && returns.All(expression =>
                    UsesOperation(semanticModel.GetOperation(expression), model, declaration, semanticModel,
                        call => AppliesSorting(call, model))),
                repository is not null && returns.Length > 0 && returns.All(expression =>
                    UsesOperation(semanticModel.GetOperation(expression), model, declaration, semanticModel,
                        call => AppliesMapping(call, model, repository)))));
        }

        return [.. methods];
    }

    private static ITypeSymbol? GetResultModel(ITypeSymbol type, bool collectionsOnly)
    {
        if (!collectionsOnly && type.AllInterfaces.Any(contract =>
                contract.ToDisplayString() == "PANiXiDA.Core.Application.Querying.IReadModel"))
        {
            return type;
        }

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
            return GetResultModel(named.TypeArguments[0], collectionsOnly);
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

    private static bool UsesOperation(IOperation? operation, ITypeSymbol model, MethodDeclarationSyntax declaration,
        SemanticModel semanticModel, Func<IInvocationOperation, bool> matches)
    {
        switch (operation)
        {
            case IAwaitOperation awaited:
                return UsesOperation(awaited.Operation, model, declaration, semanticModel, matches);
            case IConversionOperation conversion:
                return UsesOperation(conversion.Operand, model, declaration, semanticModel, matches);
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
                return UsesOperation(value, model, declaration, semanticModel, matches);
            case IInvocationOperation call:
                if (matches(call))
                {
                    return true;
                }

                return ((call.TargetMethod.Name == "ApplySorting" && IsSortingFor(call.TargetMethod.ContainingType, model)) ||
                    (call.TargetMethod.IsExtensionMethod && call.TargetMethod.Name is
                        "ToListAsync" or "ToArrayAsync" or "ToList" or "ToArray" or "Where" or "Take" or "Skip" or
                        "AsEnumerable" or "FirstOrDefaultAsync" or "SingleOrDefaultAsync" or "FirstAsync" or "SingleAsync")) &&
                    UsesOperation(call.Arguments.FirstOrDefault()?.Value, model, declaration, semanticModel, matches);
            default:
                return false;
        }
    }

    private static bool AppliesSorting(IInvocationOperation call, ITypeSymbol model)
    {
        return (call.TargetMethod.Name == "ApplySorting" && IsSortingFor(call.TargetMethod.ContainingType, model)) ||
            (call.TargetMethod.Name == "GetPagedResultAsync" && IsEfRepository(call.TargetMethod.ContainingType) &&
                call.TargetMethod.TypeArguments.Length == 3 && IsSortingFor(call.TargetMethod.TypeArguments[2], model));
    }

    private static bool AppliesMapping(IInvocationOperation call, ITypeSymbol model, INamedTypeSymbol repository)
    {
        var method = call.TargetMethod;
        var mapper = method.Name == "ProjectTo" ? method.ContainingType :
            IsEfRepository(method.ContainingType) && method.Name is "GetByIdAsync" or "GetPagedResultAsync" &&
            method.TypeArguments.Length >= 2 ? method.TypeArguments[1] : null;

        return mapper is not null && mapper.AllInterfaces.Any(contract =>
            contract.MetadataName == "IReadModelMapper`3" &&
            contract.ContainingNamespace.ToDisplayString() == "PANiXiDA.Core.Infrastructure.Persistence.Ef.Read.Mapping" &&
            SymbolEqualityComparer.Default.Equals(contract.TypeArguments[0], repository.TypeArguments[1]) &&
            SymbolEqualityComparer.Default.Equals(contract.TypeArguments[1], repository.TypeArguments[2]) &&
            SymbolEqualityComparer.Default.Equals(contract.TypeArguments[2], model));
    }

    private static bool IsEfRepository(INamedTypeSymbol type)
    {
        return type.MetadataName == "EfReadRepository`3" &&
            type.ContainingNamespace.ToDisplayString() == "PANiXiDA.Core.Infrastructure.Persistence.Ef.Read";
    }

    private static bool IsSortingFor(ITypeSymbol type, ITypeSymbol model)
    {
        return type.AllInterfaces.Any(contract => contract.OriginalDefinition.ToDisplayString() ==
            "PANiXiDA.Core.Infrastructure.Persistence.Ef.Read.Sorting.IReadModelSorting<TReadModel>" &&
            SymbolEqualityComparer.Default.Equals(contract.TypeArguments[0], model));
    }

    private sealed record ReadMethod(string Name, string Model, bool IsCollection, bool IsEfRepository,
        bool AppliesSorting, bool AppliesMapping);
}
