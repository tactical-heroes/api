using System.Xml.Linq;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Modules;

public sealed class ProjectReferenceDependencyTests
{
    private const string SourceDirectoryName = "src";

    [Fact(DisplayName = "Module project references should follow allowed dependencies when validated")]
    public void ModuleProjectReferences_Should_FollowAllowedDependencies_When_Validated()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectPaths = GetProjectPaths(repositoryRoot);
        var modules = ArchitectureDefinition.s_modules;
        var violations = modules
            .SelectMany(module => GetModuleAssemblyNames(module)
                .SelectMany(sourceAssemblyName => GetProjectReferences(
                    projectPaths[sourceAssemblyName])
                    .Where(targetAssemblyName => !IsAllowedDependency(
                        module,
                        sourceAssemblyName,
                        targetAssemblyName))
                    .Select(targetAssemblyName =>
                        $"{sourceAssemblyName} must not reference "
                            + $"{targetAssemblyName}.")))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(modules);
        Assert.True(
            violations.Length == 0,
            $"Forbidden module project references:{Environment.NewLine}"
                + string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Contracts projects should not reference other projects when validated")]
    public void ContractsProjects_Should_NotReferenceOtherProjects_When_Validated()
    {
        var projectPaths = GetProjectPaths(FindRepositoryRoot());
        var modules = ArchitectureDefinition.s_modules;

        Assert.NotEmpty(modules);
        foreach (var module in modules)
        {
            Assert.Empty(GetProjectReferences(projectPaths[module.ContractsAssemblyName]));
        }
    }

    [Fact(DisplayName = "Production project references should use contracts only from infrastructure when validated")]
    public void ProductionProjectReferences_Should_UseContractsOnlyFromInfrastructure_When_Validated()
    {
        var projectPaths = GetProjectPaths(FindRepositoryRoot());
        var contractsAssemblyNames = ArchitectureDefinition.s_modules
            .Select(module => module.ContractsAssemblyName)
            .ToHashSet(StringComparer.Ordinal);

        var violations = projectPaths
            .Where(project => !project.Key.EndsWith(".Infrastructure", StringComparison.Ordinal))
            .SelectMany(project => GetProjectReferences(project.Value)
                .Where(contractsAssemblyNames.Contains)
                .Select(target => $"{project.Key} must not reference {target}."))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(contractsAssemblyNames);
        Assert.True(
            violations.Length == 0,
            $"Forbidden Contracts project references:{Environment.NewLine}"
                + string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Common project should not reference other projects when validated")]
    public void CommonProject_Should_NotReferenceOtherProjects_When_Validated()
    {
        var projectPaths = GetProjectPaths(FindRepositoryRoot());

        Assert.Empty(GetProjectReferences(projectPaths[ArchitectureDefinition.CommonAssemblyName]));
    }

    private static Dictionary<string, string> GetProjectPaths(
        string repositoryRoot)
    {
        var sourceRoot = Path.Combine(
            repositoryRoot,
            SourceDirectoryName);

        return Directory
            .EnumerateFiles(
                sourceRoot,
                "*.csproj",
                SearchOption.AllDirectories)
            .Select(
                projectPath => new
                {
                    Name = Path.GetFileNameWithoutExtension(projectPath)
                        ?? throw new InvalidOperationException(
                            $"Could not determine project name for "
                                + $"'{projectPath}'."),
                    Path = projectPath
                })
            .ToDictionary(
                project => project.Name,
                project => project.Path,
                StringComparer.Ordinal);
    }

    private static IEnumerable<string> GetProjectReferences(
        string projectPath)
    {
        var projectDirectory = Path.GetDirectoryName(projectPath)
            ?? throw new InvalidOperationException(
                $"Could not determine directory for '{projectPath}'.");
        var project = XDocument.Load(projectPath);

        return project
            .Descendants()
            .Where(element =>
                element.Name.LocalName == "ProjectReference")
            .Select(element => element.Attribute("Include")?.Value)
            .Where(include => !string.IsNullOrWhiteSpace(include))
            .Cast<string>()
            .Select(include => Path.GetFullPath(
                Path.Combine(
                    projectDirectory,
                    include
                        .Replace(
                            '\\',
                            Path.DirectorySeparatorChar)
                        .Replace(
                            '/',
                            Path.DirectorySeparatorChar))))
            .Select(Path.GetFileNameWithoutExtension)
            .Where(projectName => !string.IsNullOrWhiteSpace(projectName))
            .Cast<string>();
    }

    private static bool IsAllowedDependency(
        ModuleArchitecture module,
        string sourceAssemblyName,
        string targetAssemblyName)
    {
        if (sourceAssemblyName == module.ContractsAssemblyName
            || sourceAssemblyName == module.DomainAssemblyName)
        {
            return false;
        }

        if (targetAssemblyName == ArchitectureDefinition.CommonAssemblyName)
        {
            return true;
        }

        if (targetAssemblyName.EndsWith(
            ".Contracts",
            StringComparison.Ordinal))
        {
            return sourceAssemblyName == module.InfrastructureAssemblyName;
        }

        if (sourceAssemblyName == module.ApplicationAssemblyName)
        {
            return targetAssemblyName == module.DomainAssemblyName;
        }

        if (sourceAssemblyName == module.InfrastructureAssemblyName)
        {
            return targetAssemblyName == module.DomainAssemblyName
                || targetAssemblyName == module.ApplicationAssemblyName;
        }

        if (sourceAssemblyName == module.PresentationAssemblyName)
        {
            return targetAssemblyName == module.ApplicationAssemblyName;
        }

        return false;
    }

    private static IReadOnlyCollection<string> GetModuleAssemblyNames(
        ModuleArchitecture module)
    {
        return
        [
            module.ContractsAssemblyName,
            module.DomainAssemblyName,
            module.ApplicationAssemblyName,
            module.InfrastructureAssemblyName,
            module.PresentationAssemblyName
        ];
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (Directory.Exists(Path.Combine(
                directory.FullName,
                SourceDirectoryName)))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException(
            $"Could not find repository root containing "
                + $"the '{SourceDirectoryName}' directory.");
    }
}
