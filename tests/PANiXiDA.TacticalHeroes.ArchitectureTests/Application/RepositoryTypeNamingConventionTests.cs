using System.Reflection;

using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.Core.Domain.Abstractions;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Application;

public sealed class RepositoryTypeNamingConventionTests
{
    private const string AbstractionsNamespaceSegment = "Abstractions";
    private const string ReadRepositorySuffix = "ReadRepository";
    private const string RepositorySuffix = "Repository";

    [Fact(DisplayName = "Repository interfaces should match feature and abstraction when declared")]
    public void RepositoryInterfaces_Should_MatchFeatureAndAbstraction_When_Declared()
    {
        var repositoryContracts = GetRepositoryContracts();
        var violations = repositoryContracts
            .Where(contract =>
                !string.Equals(
                    contract.Interface.Name,
                    contract.ExpectedInterfaceName,
                    StringComparison.Ordinal))
            .Select(contract =>
                $"{contract.Interface.FullName} must be named " +
                $"'{contract.ExpectedInterfaceName}'.")
            .ToArray();

        Assert.NotEmpty(repositoryContracts);
        Assert.True(
            violations.Length == 0,
            $"Repository interface naming violations:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Repository implementations should match interface names when declared")]
    public void RepositoryImplementations_Should_MatchInterfaceNames_When_Declared()
    {
        var repositoryContracts = GetRepositoryContracts();
        var violations = repositoryContracts
            .Where(contract =>
                contract.Implementations.Length != 1 ||
                !string.Equals(
                    contract.Implementations[0].Name,
                    contract.ExpectedImplementationName,
                    StringComparison.Ordinal))
            .Select(contract =>
                $"{contract.Interface.FullName} must have exactly one concrete " +
                $"Infrastructure implementation named " +
                $"'{contract.ExpectedImplementationName}', found: " +
                FormatImplementations(contract.Implementations))
            .ToArray();

        Assert.NotEmpty(repositoryContracts);
        Assert.True(
            violations.Length == 0,
            $"Repository implementation naming violations:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    private static RepositoryContract[] GetRepositoryContracts()
    {
        var productionAssemblies = ArchitectureDefinition.ProductionAssemblies
            .ToDictionary(
                assembly => assembly.GetName().Name
                    ?? throw new InvalidOperationException(
                        $"Could not determine the name of assembly '{assembly.FullName}'."),
                StringComparer.Ordinal);

        return
        [
            .. ArchitectureDefinition.Modules
                .SelectMany(module =>
                    GetRepositoryContracts(module, productionAssemblies))
                .OrderBy(contract => contract.Interface.FullName, StringComparer.Ordinal)
        ];
    }

    private static IEnumerable<RepositoryContract> GetRepositoryContracts(
        ModuleArchitecture module,
        IReadOnlyDictionary<string, Assembly> productionAssemblies)
    {
        var applicationAssembly = productionAssemblies[module.ApplicationAssemblyName];
        var infrastructureAssembly = productionAssemblies[module.InfrastructureAssemblyName];
        var implementationTypes = infrastructureAssembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .ToArray();

        return applicationAssembly
            .GetTypes()
            .Where(type => type.IsInterface)
            .Select(repositoryInterface => new
            {
                Interface = repositoryInterface,
                Suffix = GetRepositorySuffix(repositoryInterface)
            })
            .Where(candidate => candidate.Suffix is not null)
            .Select(candidate =>
            {
                var featureName = GetFeatureName(candidate.Interface);
                var expectedImplementationName =
                    featureName + candidate.Suffix;

                return new RepositoryContract(
                    Interface: candidate.Interface,
                    ExpectedInterfaceName: "I" + expectedImplementationName,
                    ExpectedImplementationName: expectedImplementationName,
                    Implementations:
                    [
                        .. implementationTypes.Where(
                            candidate.Interface.IsAssignableFrom)
                    ]);
            });
    }

    private static string? GetRepositorySuffix(Type repositoryInterface)
    {
        if (ImplementsOpenGeneric(
                repositoryInterface,
                typeof(IReadRepository<>)))
        {
            return ReadRepositorySuffix;
        }

        return ImplementsOpenGeneric(
            repositoryInterface,
            typeof(IRepository<,>))
            ? RepositorySuffix
            : null;
    }

    private static bool ImplementsOpenGeneric(
        Type type,
        Type openGenericType)
    {
        return type
            .GetInterfaces()
            .Append(type)
            .Any(candidate =>
                candidate.IsGenericType &&
                candidate.GetGenericTypeDefinition() == openGenericType);
    }

    private static string GetFeatureName(Type repositoryInterface)
    {
        var repositoryNamespace = repositoryInterface.Namespace
            ?? throw new InvalidOperationException(
                $"Repository interface '{repositoryInterface.FullName}' " +
                $"does not have a namespace.");
        var namespaceSegments = repositoryNamespace.Split('.');
        var abstractionsIndex = Array.LastIndexOf(
            namespaceSegments,
            AbstractionsNamespaceSegment);

        if (abstractionsIndex < 1)
        {
            throw new InvalidOperationException(
                $"Repository interface '{repositoryInterface.FullName}' must be " +
                $"declared in an '*.{{Feature}}.{AbstractionsNamespaceSegment}' " +
                $"namespace.");
        }

        return namespaceSegments[abstractionsIndex - 1];
    }

    private static string FormatImplementations(
        IReadOnlyCollection<Type> implementations)
    {
        return implementations.Count == 0
            ? "<none>"
            : string.Join(
                ", ",
                implementations
                    .Select(type => type.FullName)
                    .OrderBy(name => name, StringComparer.Ordinal));
    }

    private sealed record RepositoryContract(
        Type Interface,
        string ExpectedInterfaceName,
        string ExpectedImplementationName,
        Type[] Implementations);
}
