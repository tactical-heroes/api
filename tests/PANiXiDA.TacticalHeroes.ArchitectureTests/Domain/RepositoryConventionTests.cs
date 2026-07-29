using System.Reflection;

using PANiXiDA.Core.Domain.Abstractions;
using PANiXiDA.Core.Domain.AggregateRoots;
using PANiXiDA.Core.Domain.Identifiers;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Domain;

public sealed class RepositoryConventionTests
{
    private const string AbstractionsNamespaceSegment = "Abstractions";
    private const string ApplicationAssemblySuffix = ".Application";
    private const string DomainAssemblySuffix = ".Domain";
    private const string RepositorySuffix = "Repository";
    private const string SourceDirectoryName = "src";

    [Fact(DisplayName = "Repositories should reside in Domain abstractions when declared")]
    public void Repositories_Should_ResideInDomainAbstractions_When_Declared()
    {
        var repositoryRoot = FindRepositoryRoot();
        var repositories = GetRepositories();
        var violations = repositories
            .SelectMany(repository => GetLocationViolations(
                repositoryRoot,
                repository.Type))
            .ToArray();

        Assert.NotEmpty(repositories);
        Assert.True(
            violations.Length == 0,
            $"Repository location violations:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Repositories should use strongly typed ids and aggregate roots when declared")]
    public void Repositories_Should_UseStronglyTypedIdsAndAggregateRoots_When_Declared()
    {
        var repositories = GetRepositories();
        var violations = repositories
            .SelectMany(GetBoundaryViolations)
            .ToArray();

        Assert.NotEmpty(repositories);
        Assert.True(
            violations.Length == 0,
            $"Repository boundary type violations:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Repositories should match feature names when declared")]
    public void Repositories_Should_MatchFeatureNames_When_Declared()
    {
        var repositories = GetRepositories();
        var violations = repositories
            .Select(repository => new
            {
                repository.Type,
                ExpectedName =
                    "I" + GetFeatureName(repository.Type) + RepositorySuffix
            })
            .Where(repository => !string.Equals(
                repository.Type.Name,
                repository.ExpectedName,
                StringComparison.Ordinal))
            .Select(repository =>
                $"{repository.Type.FullName} must be named " +
                $"'{repository.ExpectedName}'.")
            .ToArray();

        Assert.NotEmpty(repositories);
        Assert.True(
            violations.Length == 0,
            $"Repository interface naming violations:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Repository constructor parameters should follow type-based naming when repository is injected")]
    public void ConstructorParameters_Should_FollowTypeBasedNaming_When_RepositoryIsInjected()
    {
        var repositoryParameters = ArchitectureDefinition.ProductionAssemblies
            .Where(assembly => assembly.GetName().Name?.EndsWith(
                ApplicationAssemblySuffix,
                StringComparison.Ordinal) == true)
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .SelectMany(type => type
                .GetConstructors(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic)
                .SelectMany(constructor => constructor
                    .GetParameters()
                    .Where(parameter => GetClosedRepositoryContract(
                        parameter.ParameterType) is not null)
                    .Select(parameter => new ConstructorParameter(
                        type,
                        parameter))))
            .OrderBy(candidate => candidate.DeclaringType.FullName, StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(repositoryParameters);

        var violations = repositoryParameters
            .Select(candidate => new
            {
                candidate.DeclaringType,
                candidate.Parameter,
                ExpectedName = GetExpectedParameterName(
                    candidate.Parameter.ParameterType)
            })
            .Where(candidate => !string.Equals(
                candidate.Parameter.Name,
                candidate.ExpectedName,
                StringComparison.Ordinal))
            .Select(candidate =>
                $"{candidate.DeclaringType.FullName} constructor parameter " +
                $"'{candidate.Parameter.Name}' of type " +
                $"'{candidate.Parameter.ParameterType.FullName}' must be named " +
                $"'{candidate.ExpectedName}'.")
            .ToArray();

        Assert.True(
            violations.Length == 0,
            string.Join(Environment.NewLine, violations));
    }

    private static Repository[] GetRepositories()
    {
        return
        [
            .. ArchitectureDefinition.ProductionAssemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsInterface)
                .Select(type => new
                {
                    Type = type,
                    Contract = GetClosedRepositoryContract(type)
                })
                .Where(repository => repository.Contract is not null)
                .Select(repository => new Repository(
                    repository.Type,
                    repository.Contract!))
                .OrderBy(
                    repository => repository.Type.FullName,
                    StringComparer.Ordinal)
        ];
    }

    private static IEnumerable<string> GetLocationViolations(
        string repositoryRoot,
        Type repository)
    {
        var violations = new List<string>();
        var assemblyName = GetAssemblyName(repository);

        if (!assemblyName.EndsWith(
                DomainAssemblySuffix,
                StringComparison.Ordinal))
        {
            violations.Add(
                $"{repository.FullName} inherits IRepository<,> and must be " +
                $"declared in Domain.");
        }

        if (!HasNamespaceSegment(
                repository,
                AbstractionsNamespaceSegment))
        {
            violations.Add(
                $"{repository.FullName} must be declared in an " +
                $"'{AbstractionsNamespaceSegment}' namespace.");
        }

        var expectedSourceFilePath = GetExpectedSourceFilePath(
            repositoryRoot,
            repository);

        if (!File.Exists(expectedSourceFilePath))
        {
            violations.Add(
                $"{repository.FullName} must have a matching source file at " +
                $"'{Path.GetRelativePath(
                    repositoryRoot,
                    expectedSourceFilePath)}'.");
        }

        return violations;
    }

    private static IEnumerable<string> GetBoundaryViolations(
        Repository repository)
    {
        var genericArguments = repository.Contract.GetGenericArguments();
        var identifierType = genericArguments[0];
        var aggregateType = genericArguments[1];
        var violations = new List<string>();

        if (IsPrimitiveValue(identifierType) ||
            !typeof(IStronglyTypedId).IsAssignableFrom(identifierType))
        {
            violations.Add(
                $"{repository.Type.FullName} must use a non-primitive " +
                $"strongly typed ID, found '{identifierType.FullName}'.");
        }

        if (IsPrimitiveValue(aggregateType) ||
            !typeof(IAggregateRoot).IsAssignableFrom(aggregateType))
        {
            violations.Add(
                $"{repository.Type.FullName} must use an aggregate root, " +
                $"found '{aggregateType.FullName}'.");
        }

        violations.AddRange(repository.Type
            .GetMethods()
            .Where(MethodContainsPrimitiveValue)
            .Select(method =>
                $"{repository.Type.FullName}.{method.Name} must not use " +
                $"primitive parameters or return types."));

        return violations;
    }

    private static Type? GetClosedRepositoryContract(Type type)
    {
        return type
            .GetInterfaces()
            .Append(type)
            .FirstOrDefault(candidate =>
                candidate.IsGenericType &&
                candidate.GetGenericTypeDefinition() ==
                typeof(IRepository<,>));
    }

    private static bool MethodContainsPrimitiveValue(MethodInfo method)
    {
        return ContainsPrimitiveValue(
                   method.ReturnType,
                   new HashSet<Type>()) ||
               method.GetParameters().Any(parameter =>
                   ContainsPrimitiveValue(
                       parameter.ParameterType,
                       new HashSet<Type>()));
    }

    private static bool ContainsPrimitiveValue(
        Type type,
        ISet<Type> visitedTypes)
    {
        if (!visitedTypes.Add(type))
        {
            return false;
        }

        if (IsPrimitiveValue(type))
        {
            return true;
        }

        if (type.HasElementType)
        {
            return ContainsPrimitiveValue(
                type.GetElementType()
                    ?? throw new InvalidOperationException(
                        $"Could not determine element type for '{type}'."),
                visitedTypes);
        }

        return type.IsGenericType &&
               type.GetGenericArguments().Any(argument =>
                   ContainsPrimitiveValue(argument, visitedTypes));
    }

    private static bool IsPrimitiveValue(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType.IsPrimitive ||
               underlyingType.IsEnum ||
               underlyingType == typeof(string) ||
               underlyingType == typeof(decimal) ||
               underlyingType == typeof(Guid) ||
               underlyingType == typeof(DateTime) ||
               underlyingType == typeof(DateTimeOffset) ||
               underlyingType == typeof(DateOnly) ||
               underlyingType == typeof(TimeOnly) ||
               underlyingType == typeof(TimeSpan);
    }

    private static string GetFeatureName(Type repository)
    {
        var repositoryNamespace = repository.Namespace
            ?? throw new InvalidOperationException(
                $"Repository interface '{repository.FullName}' does not have " +
                $"a namespace.");
        var namespaceSegments = repositoryNamespace.Split('.');
        var abstractionsIndex = Array.LastIndexOf(
            namespaceSegments,
            AbstractionsNamespaceSegment);

        if (abstractionsIndex < 1)
        {
            throw new InvalidOperationException(
                $"Repository interface '{repository.FullName}' must be " +
                $"declared in an '*.{{Feature}}." +
                $"{AbstractionsNamespaceSegment}' namespace.");
        }

        return namespaceSegments[abstractionsIndex - 1];
    }

    private static string GetExpectedParameterName(Type parameterType)
    {
        var typeName = RemoveGenericArity(parameterType.Name);
        var nameWithoutInterfacePrefix = typeName[1..];

        return char.ToLowerInvariant(nameWithoutInterfacePrefix[0]) +
               nameWithoutInterfacePrefix[1..];
    }

    private static string RemoveGenericArity(string typeName)
    {
        var genericAritySeparatorIndex =
            typeName.IndexOf('`', StringComparison.Ordinal);

        return genericAritySeparatorIndex < 0
            ? typeName
            : typeName[..genericAritySeparatorIndex];
    }

    private static bool HasNamespaceSegment(
        Type type,
        string namespaceSegment)
    {
        return type.Namespace?
            .Split('.')
            .Contains(namespaceSegment, StringComparer.Ordinal) == true;
    }

    private static string GetExpectedSourceFilePath(
        string repositoryRoot,
        Type repository)
    {
        var assemblyName = GetAssemblyName(repository);
        var module = ArchitectureDefinition.Modules.Single(candidate =>
            string.Equals(
                candidate.ContractsAssemblyName,
                assemblyName,
                StringComparison.Ordinal) ||
            string.Equals(
                candidate.DomainAssemblyName,
                assemblyName,
                StringComparison.Ordinal) ||
            string.Equals(
                candidate.ApplicationAssemblyName,
                assemblyName,
                StringComparison.Ordinal) ||
            string.Equals(
                candidate.InfrastructureAssemblyName,
                assemblyName,
                StringComparison.Ordinal) ||
            string.Equals(
                candidate.PresentationAssemblyName,
                assemblyName,
                StringComparison.Ordinal));
        var moduleDirectoryName =
            module.Name[(module.Name.LastIndexOf('.') + 1)..];
        var repositoryNamespace = repository.Namespace
            ?? throw new InvalidOperationException(
                $"Repository interface '{repository.FullName}' does not have " +
                $"a namespace.");
        var namespacePrefix = assemblyName + ".";
        var relativeNamespace = repositoryNamespace.StartsWith(
            namespacePrefix,
            StringComparison.Ordinal)
            ? repositoryNamespace[namespacePrefix.Length..]
            : throw new InvalidOperationException(
                $"Repository interface '{repository.FullName}' namespace must " +
                $"start with '{namespacePrefix}'.");

        return Path.Combine(
            repositoryRoot,
            SourceDirectoryName,
            moduleDirectoryName,
            assemblyName,
            relativeNamespace.Replace(
                '.',
                Path.DirectorySeparatorChar),
            $"{repository.Name.Split('`')[0]}.cs");
    }

    private static string GetAssemblyName(Type type)
    {
        return type.Assembly.GetName().Name
            ?? throw new InvalidOperationException(
                $"Could not determine assembly for '{type.FullName}'.");
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
            $"Could not find repository root containing " +
            $"the '{SourceDirectoryName}' directory.");
    }

    private sealed record Repository(
        Type Type,
        Type Contract);

    private sealed record ConstructorParameter(
        Type DeclaringType,
        ParameterInfo Parameter);
}
