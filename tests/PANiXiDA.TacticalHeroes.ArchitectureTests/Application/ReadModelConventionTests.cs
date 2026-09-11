using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using PANiXiDA.Core.Application.Messaging.Mediator.Handlers;
using PANiXiDA.Core.Application.Querying;
using PANiXiDA.TacticalHeroes.ArchitectureTests.Global;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Application;

public sealed class ReadModelConventionTests
{
    private const string ApplicationAssemblySuffix = ".Application";
    private const string ReadModelSuffix = "ReadModel";

    [Fact(DisplayName = "Read models should be records when declared")]
    public async Task ReadModels_Should_BeRecords_When_Declared()
    {
        var readModels = await ProductionSourceDocumentDiscovery.GetItemsAsync(async (_, document) =>
        {
            if (document.Project.AssemblyName?.EndsWith(ApplicationAssemblySuffix, StringComparison.Ordinal) != true)
            {
                return Array.Empty<INamedTypeSymbol>();
            }

            var syntaxRoot = await document.GetSyntaxRootAsync();
            var semanticModel = await document.GetSemanticModelAsync();
            var readModelContract = semanticModel?.Compilation.GetTypeByMetadataName(
                "PANiXiDA.Core.Application.Querying.IReadModel");

            return syntaxRoot is null || semanticModel is null
                ? []
                : syntaxRoot.DescendantNodes()
                    .OfType<TypeDeclarationSyntax>()
                    .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
                    .OfType<INamedTypeSymbol>()
                    .Where(type => type.TypeKind is TypeKind.Class or TypeKind.Struct &&
                        (type.Name.EndsWith(ReadModelSuffix, StringComparison.Ordinal) ||
                         type.AllInterfaces.Any(contract =>
                             SymbolEqualityComparer.Default.Equals(contract, readModelContract))))
                    .ToArray();
        });
        var violations = readModels
            .Where(type => !type.IsRecord)
            .Select(type => $"{type.ToDisplayString()} must be a record.")
            .ToArray();

        Assert.NotEmpty(readModels);
        Assert.True(
            violations.Length == 0,
            $"Read model record violations:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Read models should end with ReadModel when declared")]
    public void ReadModels_Should_EndWithReadModel_When_Declared()
    {
        var readModels = GetApplicationTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false } &&
                typeof(IReadModel).IsAssignableFrom(type))
            .ToArray();
        var violations = readModels
            .Where(type => !type.Name.EndsWith(
                ReadModelSuffix,
                StringComparison.Ordinal))
            .Select(type =>
                $"{type.FullName} implements IReadModel and must end with " +
                $"'{ReadModelSuffix}'.")
            .ToArray();

        Assert.NotEmpty(readModels);
        Assert.True(
            violations.Length == 0,
            $"Read model naming violations:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Types ending with ReadModel should implement IReadModel when declared")]
    public void TypesEndingWithReadModel_Should_ImplementIReadModel_When_Declared()
    {
        var namedReadModels = GetApplicationTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false } &&
                type.Name.EndsWith(
                    ReadModelSuffix,
                    StringComparison.Ordinal))
            .ToArray();
        var violations = namedReadModels
            .Where(type => !typeof(IReadModel).IsAssignableFrom(type))
            .Select(type =>
                $"{type.FullName} ends with '{ReadModelSuffix}' and must " +
                $"implement IReadModel.")
            .ToArray();

        Assert.NotEmpty(namedReadModels);
        Assert.True(
            violations.Length == 0,
            $"Read model inheritance violations:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Query handlers should return read models when declared")]
    public void QueryHandlers_Should_ReturnReadModels_When_Declared()
    {
        var queryHandlers = GetApplicationTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .SelectMany(type => type
                .GetInterfaces()
                .Where(candidate =>
                    candidate.IsGenericType &&
                    candidate.GetGenericTypeDefinition() ==
                    typeof(IQueryHandler<,>))
                .Select(contract => new QueryHandler(
                    type,
                    contract.GetGenericArguments()[1])))
            .ToArray();
        var violations = queryHandlers
            .Where(handler => !ReadSideConvention.IsReadModelResult(
                handler.ResultType))
            .Select(handler =>
                $"{handler.Type.FullName} must return an IReadModel, " +
                $"optionally wrapped in Task, Result, a collection, or a " +
                $"pagination model; found '{handler.ResultType}'.")
            .ToArray();

        Assert.NotEmpty(queryHandlers);
        Assert.True(
            violations.Length == 0,
            $"Query handler result type violations:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    private static Type[] GetApplicationTypes()
    {
        return
        [
            .. ArchitectureDefinition.ProductionAssemblies
                .Where(assembly => assembly.GetName().Name?.EndsWith(
                    ApplicationAssemblySuffix,
                    StringComparison.Ordinal) == true)
                .SelectMany(assembly => assembly.GetTypes())
                .OrderBy(type => type.FullName, StringComparer.Ordinal)
        ];
    }

    private sealed record QueryHandler(
        Type Type,
        Type ResultType);
}
