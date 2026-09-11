using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using PANiXiDA.TacticalHeroes.ArchitectureTests.Global;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Application;

public sealed class AggregateFilterConventionTests
{
    [Fact(DisplayName = "Aggregate roots should have common filter records and validators when declared")]
    public async Task AggregateRoots_Should_HaveCommonFilterRecordsAndValidators_When_Declared()
    {
        var sourceTypes = await ProductionSourceDocumentDiscovery.GetItemsAsync(GetApplicationTypesAsync);
        var aggregates = ArchitectureDefinition.Modules
            .SelectMany(module => InfrastructurePersistenceConvention.GetAggregateRootTypes(module)
                .Select(aggregate => (Module: module, Aggregate: aggregate)))
            .ToArray();

        var violations = aggregates
            .SelectMany(candidate => GetViolations(candidate.Module, candidate.Aggregate, sourceTypes))
            .ToArray();

        Assert.NotEmpty(aggregates);
        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    private static async Task<ApplicationType[]> GetApplicationTypesAsync(string repositoryRoot, Document document)
    {
        if (document.Project.AssemblyName?.EndsWith(".Application", StringComparison.Ordinal) != true)
        {
            return [];
        }

        var syntaxRoot = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();

        return syntaxRoot is null || semanticModel is null
            ? []
            : [.. syntaxRoot.DescendantNodes()
                .OfType<TypeDeclarationSyntax>()
                .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
                .OfType<INamedTypeSymbol>()
                .Select(type => new ApplicationType(repositoryRoot, type))];
    }

    private static IEnumerable<string> GetViolations(
        ModuleArchitecture module,
        Type aggregate,
        ApplicationType[] sourceTypes)
    {
        var pluralName = EnglishNamingConvention.Pluralize(aggregate.Name);
        var filterName = $"{module.ApplicationAssemblyName}.{pluralName}.Common.Filters.{pluralName}Filter";
        var filter = FindType(sourceTypes, module, filterName);

        if (filter is null)
        {
            yield return $"{aggregate.FullName}: missing filter '{filterName}'.";
            yield break;
        }

        if (!filter.Type.IsRecord || filter.Type.TypeKind != TypeKind.Class || filter.Type.IsAbstract ||
            filter.Type.Arity != 0 || !filter.Type.AllInterfaces.Any(contract =>
                contract.ToDisplayString() == "PANiXiDA.Core.Application.Querying.Filtering.IFilter" &&
                contract.ContainingAssembly.Name == "PANiXiDA.Core.Application"))
        {
            yield return $"{filterName}: must be a concrete, non-generic record implementing IFilter.";
        }

        foreach (var violation in GetLocationViolations(module, pluralName, filter))
        {
            yield return violation;
        }

        var validatorName = filterName + "Validator";
        var validator = FindType(sourceTypes, module, validatorName);

        if (validator is null)
        {
            yield return $"{filterName}: missing validator '{validatorName}'.";
            yield break;
        }

        if (validator.Type.TypeKind != TypeKind.Class || validator.Type.IsAbstract ||
            validator.Type.Arity != 0 || !validator.Type.AllInterfaces.Any(contract =>
                contract.OriginalDefinition.ToDisplayString() == "FluentValidation.IValidator<T>" &&
                contract.ContainingAssembly.Name == "FluentValidation" &&
                SymbolEqualityComparer.Default.Equals(contract.TypeArguments[0], filter.Type)))
        {
            yield return $"{validatorName}: must be a concrete, non-generic IValidator<{pluralName}Filter>.";
        }

        foreach (var violation in GetLocationViolations(module, pluralName, validator))
        {
            yield return violation;
        }
    }

    private static ApplicationType? FindType(ApplicationType[] sourceTypes, ModuleArchitecture module, string name)
    {
        return sourceTypes.FirstOrDefault(source =>
            source.Type.ContainingAssembly.Name == module.ApplicationAssemblyName &&
            source.Type.ToDisplayString() == name);
    }

    private static IEnumerable<string> GetLocationViolations(
        ModuleArchitecture module,
        string pluralName,
        ApplicationType source)
    {
        var expectedPath = Path.Combine(
            source.RepositoryRoot, "src", InfrastructurePersistenceConvention.GetModuleShortName(module),
            module.ApplicationAssemblyName, pluralName, "Common", "Filters", $"{source.Type.Name}.cs");

        if (!source.Type.DeclaringSyntaxReferences.Any(reference =>
                string.Equals(reference.SyntaxTree.FilePath, expectedPath, StringComparison.Ordinal)))
        {
            yield return $"{source.Type}: must be declared in '{Path.GetRelativePath(source.RepositoryRoot, expectedPath)}'.";
        }
    }

    private sealed record ApplicationType(string RepositoryRoot, INamedTypeSymbol Type);
}
