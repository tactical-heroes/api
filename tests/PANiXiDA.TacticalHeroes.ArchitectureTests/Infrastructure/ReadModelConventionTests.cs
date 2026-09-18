using System.Reflection;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using PANiXiDA.Core.Infrastructure.Persistence.Ef.Read.Mapping;
using PANiXiDA.Core.Infrastructure.Persistence.Ef.Read.Models;
using PANiXiDA.Core.Infrastructure.Persistence.Ef.Read.Sorting;
using PANiXiDA.TacticalHeroes.ArchitectureTests.Global;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Infrastructure;

public sealed class ReadModelConventionTests
{
    private const string ReadDbModelSuffix = "ReadDbModel";

    [Theory(DisplayName = "Read model components should be internal sealed classes when declared")]
    [InlineData(typeof(IReadModelMapper<,,>))]
    [InlineData(typeof(IReadModelSorting<>))]
    public void ReadModelComponents_Should_BeInternalSealedClasses_When_Declared(Type contractType)
    {
        var components = ArchitectureDefinition.ProductionAssemblies
            .Where(assembly => assembly.GetName().Name?.EndsWith(".Infrastructure", StringComparison.Ordinal) == true)
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsInterface &&
                InfrastructurePersistenceConvention.GetClosedGenericInterface(type, contractType) is not null)
            .ToArray();
        var violations = components
            .Where(type => type is not { IsClass: true, IsNotPublic: true, IsNested: false, IsSealed: true, IsAbstract: false })
            .Select(type => $"{type.FullName} must be a top-level internal sealed class.")
            .ToArray();

        Assert.NotEmpty(components);
        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    [Theory(DisplayName = "Read model components should match model names when declared")]
    [InlineData(typeof(IReadModelMapper<,,>), "Mapper")]
    [InlineData(typeof(IReadModelSorting<>), "Sorting")]
    public void ReadModelComponents_Should_MatchModelNames_When_Declared(Type contractType, string suffix)
    {
        var components = GetReadModelComponents(contractType);
        var violations = components
            .Where(type => type.Name != GetReadModel(type, contractType).Name + suffix ||
                InfrastructurePersistenceConvention.FindSourceFiles(type)
                    .Any(path => Path.GetFileNameWithoutExtension(path) != type.Name))
            .Select(type => $"{type.FullName} must be named '{GetReadModel(type, contractType).Name + suffix}' " +
                "and declared in a file with the same name.")
            .ToArray();

        Assert.NotEmpty(components);
        Assert.True(
            violations.Length == 0,
            $"Read model component naming violations:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Theory(DisplayName = "Read model components should reside in matching Application slices when declared")]
    [InlineData(typeof(IReadModelMapper<,,>))]
    [InlineData(typeof(IReadModelSorting<>))]
    public async Task ReadModelComponents_Should_ResideInMatchingApplicationSlices_When_Declared(Type contractType)
    {
        var components = GetReadModelComponents(contractType);
        var sources = await ProductionSourceDocumentDiscovery.GetItemsAsync(GetApplicationModelsAsync);
        var violations = components
            .SelectMany(type => GetComponentLocationViolations(type, contractType, sources))
            .ToArray();

        Assert.NotEmpty(components);
        Assert.True(
            violations.Length == 0,
            $"Read model component location violations:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Read model sorting should share the mapper directory when declared")]
    public void ReadModelSorting_Should_ShareMapperDirectory_When_Declared()
    {
        var sortingTypes = GetReadModelComponents(typeof(IReadModelSorting<>));
        var mappers = GetReadModelComponents(typeof(IReadModelMapper<,,>));
        var violations = sortingTypes.Where(sorting => !mappers.Any(mapper =>
                GetReadModel(mapper, typeof(IReadModelMapper<,,>)) == GetReadModel(sorting, typeof(IReadModelSorting<>)) &&
                mapper.Namespace == sorting.Namespace &&
                InfrastructurePersistenceConvention.FindSourceFiles(mapper).Select(Path.GetDirectoryName)
                    .Intersect(InfrastructurePersistenceConvention.FindSourceFiles(sorting).Select(Path.GetDirectoryName),
                        StringComparer.OrdinalIgnoreCase).Any()))
            .Select(type => $"{type.FullName} must share its directory and namespace with a mapper for the same read model.")
            .ToArray();

        Assert.NotEmpty(sortingTypes);
        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Read database models should end with ReadDbModel when declared")]
    public void ReadDatabaseModels_Should_EndWithReadDbModel_When_Declared()
    {
        var readDbModels = GetReadDatabaseModels();
        var violations = readDbModels
            .Where(type => !type.Name.EndsWith(
                ReadDbModelSuffix,
                StringComparison.Ordinal))
            .Select(type =>
                $"{type.FullName} must end with '{ReadDbModelSuffix}'.")
            .ToArray();

        Assert.NotEmpty(readDbModels);
        Assert.True(
            violations.Length == 0,
            $"Read database model naming violations:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Read database models should reside in aggregate Read DbModels directories when declared")]
    public void ReadDatabaseModels_Should_ResideInAggregateReadDbModelsDirectories_When_Declared()
    {
        var readDbModels = GetReadDatabaseModels();
        var violations = readDbModels
            .SelectMany(type =>
                InfrastructurePersistenceConvention
                    .GetAggregateFeatureLocationViolations(
                        type,
                        "Read",
                        "DbModels"))
            .ToArray();

        Assert.NotEmpty(readDbModels);
        Assert.True(
            violations.Length == 0,
            $"Read database model location violations:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Read database model aggregate foreign keys should have bidirectional navigations when declared")]
    public void ReadDatabaseModelAggregateForeignKeys_Should_HaveBidirectionalNavigations_When_Declared()
    {
        var readDbModels = GetReadDatabaseModels();
        var violations = readDbModels
            .SelectMany(readDbModel =>
                GetAggregateNavigationViolations(
                    readDbModel,
                    readDbModels))
            .ToArray();

        Assert.True(
            violations.Length == 0,
            $"Read database model navigation violations:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    private static Type[] GetReadModelComponents(Type contractType)
    {
        return InfrastructurePersistenceConvention
            .GetConcreteInfrastructureTypes(type =>
                InfrastructurePersistenceConvention
                    .GetClosedGenericInterface(
                        type,
                        contractType) is not null);
    }

    private static Type GetReadModel(Type type, Type contractType)
    {
        return InfrastructurePersistenceConvention.GetClosedGenericInterface(type, contractType)!.GetGenericArguments()[^1];
    }

    private static async Task<ModelSource[]> GetApplicationModelsAsync(string repositoryRoot, Document document)
    {
        if (document.Project.AssemblyName?.EndsWith(".Application", StringComparison.Ordinal) != true)
        {
            return [];
        }

        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        return root is null || semanticModel is null ? [] :
            [.. root.DescendantNodes().OfType<TypeDeclarationSyntax>()
                .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
                .OfType<INamedTypeSymbol>()
                .Where(type => type.AllInterfaces.Any(contract => contract.ToDisplayString() == "PANiXiDA.Core.Application.Querying.IReadModel"))
                .Select(type => new ModelSource(type.ToDisplayString(), Path.GetDirectoryName(document.FilePath!)!))];
    }

    private static IEnumerable<string> GetComponentLocationViolations(Type component, Type contractType, ModelSource[] sources)
    {
        var model = GetReadModel(component, contractType);
        var module = InfrastructurePersistenceConvention.GetModule(component);
        var modelSources = sources.Where(source => source.Name == model.FullName).ToArray();
        if (model.Assembly.GetName().Name != module.ApplicationAssemblyName || modelSources.Length == 0)
        {
            yield return $"{component.FullName}: read model must be declared in {module.ApplicationAssemblyName}.";
            yield break;
        }

        foreach (var source in modelSources)
        {
            var slice = Path.GetFileName(source.Directory);
            var feature = Path.GetFileName(Path.GetDirectoryName(source.Directory));
            foreach (var violation in InfrastructurePersistenceConvention.GetLocationViolations(
                         component, "Persistence", "Features", feature!, "Read", slice))
            {
                yield return violation;
            }
        }

        if (contractType == typeof(IReadModelMapper<,,>))
        {
            foreach (var violation in GetMapperLocationViolations(component))
            {
                yield return violation;
            }
        }
    }

    private static Type[] GetReadDatabaseModels()
    {
        return InfrastructurePersistenceConvention
            .GetConcreteInfrastructureTypes(type =>
                InfrastructurePersistenceConvention
                    .GetClosedGenericBaseType(
                        type,
                        typeof(ReadDbModel<>)) is not null ||
                InfrastructurePersistenceConvention
                    .GetClosedGenericBaseType(
                        type,
                        typeof(AuditableReadDbModel<>)) is not null);
    }

    private static IEnumerable<string> GetMapperLocationViolations(
        Type mapper)
    {
        var mapperContract = InfrastructurePersistenceConvention
            .GetClosedGenericInterface(
                mapper,
                typeof(IReadModelMapper<,,>))
            ?? throw new InvalidOperationException(
                $"Could not determine read model mapper contract for " +
                $"'{mapper.FullName}'.");
        var readDbModelType = mapperContract.GetGenericArguments()[1];
        var aggregateName = readDbModelType.Name.EndsWith(
            ReadDbModelSuffix,
            StringComparison.Ordinal)
            ? readDbModelType.Name[..^ReadDbModelSuffix.Length]
            : readDbModelType.Name;
        var module =
            InfrastructurePersistenceConvention.GetModule(mapper);
        var aggregateType = InfrastructurePersistenceConvention
            .GetAggregateRootTypes(module)
            .SingleOrDefault(type => string.Equals(
                type.Name,
                aggregateName,
                StringComparison.Ordinal));

        if (aggregateType is null)
        {
            return
            [
                $"{mapper.FullName} maps '{readDbModelType.FullName}', which " +
                $"must correspond to an aggregate root in module " +
                $"'{module.Name}'."
            ];
        }

        var featureName =
            EnglishNamingConvention.Pluralize(aggregateType.Name);

        return InfrastructurePersistenceConvention.GetLocationViolations(
            mapper,
            "Persistence",
            "Features",
            featureName,
            "Read",
            GetReadModel(mapper, typeof(IReadModelMapper<,,>)).Namespace!.Split('.')[^1]);
    }

    private static IEnumerable<string> GetAggregateNavigationViolations(
        Type dependentReadDbModel,
        IReadOnlyCollection<Type> readDbModels)
    {
        var module =
            InfrastructurePersistenceConvention.GetModule(dependentReadDbModel);
        var aggregateNames = InfrastructurePersistenceConvention
            .GetAggregateRootTypes(module)
            .Select(type => type.Name)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var foreignKey in dependentReadDbModel
                     .GetProperties()
                     .Where(property =>
                         property.Name.Length > "Id".Length &&
                         property.Name.EndsWith("Id", StringComparison.Ordinal)))
        {
            var aggregateName = foreignKey.Name[..^"Id".Length];

            if (!aggregateNames.Contains(aggregateName))
            {
                continue;
            }

            var principalReadDbModel = readDbModels.SingleOrDefault(type =>
                type.Assembly == dependentReadDbModel.Assembly &&
                string.Equals(
                    type.Name,
                    aggregateName + ReadDbModelSuffix,
                    StringComparison.Ordinal));

            if (principalReadDbModel is null)
            {
                yield return
                    $"{dependentReadDbModel.FullName}.{foreignKey.Name} " +
                    $"references aggregate '{aggregateName}', but its " +
                    $"{ReadDbModelSuffix} is not declared in the module.";
                continue;
            }

            var referenceNavigation = dependentReadDbModel
                .GetProperties()
                .SingleOrDefault(property =>
                    property.PropertyType == principalReadDbModel);

            if (referenceNavigation is null)
            {
                yield return
                    $"{dependentReadDbModel.FullName}.{foreignKey.Name} " +
                    $"must have a reference navigation to " +
                    $"{principalReadDbModel.FullName}.";
            }
            else if (new NullabilityInfoContext()
                         .Create(referenceNavigation)
                         .ReadState != NullabilityState.Nullable)
            {
                yield return
                    $"{dependentReadDbModel.FullName}." +
                    $"{referenceNavigation.Name} reference navigation " +
                    $"must be nullable.";
            }

            if (!principalReadDbModel
                    .GetProperties()
                    .Any(property => IsCollectionOf(
                        property.PropertyType,
                        dependentReadDbModel)))
            {
                yield return
                    $"{principalReadDbModel.FullName} must have a collection " +
                    $"navigation to {dependentReadDbModel.FullName} for " +
                    $"{dependentReadDbModel.FullName}.{foreignKey.Name}.";
            }
        }
    }

    private sealed record ModelSource(string Name, string Directory);

    private static bool IsCollectionOf(
        Type propertyType,
        Type elementType)
    {
        return propertyType
            .GetInterfaces()
            .Append(propertyType)
            .Any(type =>
                type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(ICollection<>) &&
                type.GenericTypeArguments[0] == elementType);
    }
}
