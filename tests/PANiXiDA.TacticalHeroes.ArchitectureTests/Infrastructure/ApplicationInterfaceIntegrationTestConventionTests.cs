using System.Reflection;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Infrastructure;

public sealed class ApplicationInterfaceIntegrationTestConventionTests
{
    private const string IntegrationTestsAssemblySuffix = ".IntegrationTests";
    private const string InfrastructureDirectoryName = "Infrastructure";
    private const string SourceDirectoryName = "src";
    private const string TestsDirectoryName = "tests";

    [Fact(DisplayName = "Infrastructure implementations of Application interfaces should have matching integration test files")]
    public void InfrastructureImplementations_Should_HaveMatchingIntegrationTestFiles_When_ApplicationInterfacesAreImplemented()
    {
        var repositoryRoot = FindRepositoryRoot();
        var productionAssemblies = ArchitectureDefinition.ProductionAssemblies
            .ToDictionary(
                assembly => assembly.GetName().Name
                    ?? throw new InvalidOperationException(
                        $"Could not determine the name of assembly '{assembly.FullName}'."),
                StringComparer.Ordinal);
        var implementations = ArchitectureDefinition.Modules
            .SelectMany(module => GetInfrastructureImplementations(
                module,
                productionAssemblies))
            .OrderBy(target => target.Implementation.FullName, StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(implementations);

        var missingTestFiles = implementations
            .Select(target => new
            {
                target.Implementation,
                ExpectedPath = GetExpectedTestFilePath(
                    repositoryRoot,
                    target.Module,
                    target.Implementation)
            })
            .Where(target => !File.Exists(target.ExpectedPath))
            .Select(target =>
                $"{target.Implementation.FullName}: " +
                Path.GetRelativePath(repositoryRoot, target.ExpectedPath))
            .ToArray();

        Assert.True(
            missingTestFiles.Length == 0,
            $"Missing integration test files:{Environment.NewLine}" +
            string.Join(Environment.NewLine, missingTestFiles));
    }

    private static IEnumerable<InfrastructureImplementation> GetInfrastructureImplementations(
        ModuleArchitecture module,
        IReadOnlyDictionary<string, Assembly> productionAssemblies)
    {
        var applicationAssembly = productionAssemblies[module.ApplicationAssemblyName];
        var infrastructureAssembly = productionAssemblies[module.InfrastructureAssemblyName];
        var applicationInterfaces = applicationAssembly
            .GetTypes()
            .Where(type => type.IsInterface)
            .ToArray();

        return infrastructureAssembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .Where(type => applicationInterfaces.Any(
                applicationInterface => applicationInterface.IsAssignableFrom(type)))
            .Select(implementation => new InfrastructureImplementation(
                module,
                implementation));
    }

    private static string GetExpectedTestFilePath(
        string repositoryRoot,
        ModuleArchitecture module,
        Type implementation)
    {
        var implementationNamespace = implementation.Namespace
            ?? throw new InvalidOperationException(
                $"Infrastructure implementation '{implementation.FullName}' does not have a namespace.");
        var infrastructureNamespace = module.InfrastructureAssemblyName;
        var infrastructureNamespacePrefix = infrastructureNamespace + ".";
        string relativeNamespace;

        if (string.Equals(
                implementationNamespace,
                infrastructureNamespace,
                StringComparison.Ordinal))
        {
            relativeNamespace = string.Empty;
        }
        else if (implementationNamespace.StartsWith(
                     infrastructureNamespacePrefix,
                     StringComparison.Ordinal))
        {
            relativeNamespace =
                implementationNamespace[infrastructureNamespacePrefix.Length..];
        }
        else
        {
            throw new InvalidOperationException(
                $"Infrastructure implementation namespace '{implementationNamespace}' must be " +
                $"'{infrastructureNamespace}' or start with '{infrastructureNamespacePrefix}'.");
        }

        var moduleDirectoryName = module.Name[(module.Name.LastIndexOf('.') + 1)..];
        var integrationTestsAssemblyName =
            module.Name + IntegrationTestsAssemblySuffix;
        var namespacePath = relativeNamespace.Replace(
            '.',
            Path.DirectorySeparatorChar);
        var implementationName = implementation.Name.Split('`')[0];

        return Path.Combine(
            repositoryRoot,
            TestsDirectoryName,
            moduleDirectoryName,
            integrationTestsAssemblyName,
            InfrastructureDirectoryName,
            namespacePath,
            $"{implementationName}Tests.cs");
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (Directory.Exists(Path.Combine(
                    directory.FullName,
                    SourceDirectoryName)) &&
                Directory.Exists(Path.Combine(
                    directory.FullName,
                    TestsDirectoryName)))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException(
            $"Could not find repository root containing " +
            $"'{SourceDirectoryName}' and '{TestsDirectoryName}' directories.");
    }

    private sealed record InfrastructureImplementation(
        ModuleArchitecture Module,
        Type Implementation);
}
