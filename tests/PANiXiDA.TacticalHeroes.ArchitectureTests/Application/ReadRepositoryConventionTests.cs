using System.Reflection;

using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.Core.Domain.AggregateRoots;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Application;

public sealed class ReadRepositoryConventionTests
{
    private const string AbstractionsNamespaceSegment = "Abstractions";
    private const string ApplicationAssemblySuffix = ".Application";
    private const string ReadRepositorySuffix = "ReadRepository";
    private const string SourceDirectoryName = "src";

    [Fact(DisplayName = "Read repositories should reside in Application abstractions when declared")]
    public void ReadRepositories_Should_ResideInApplicationAbstractions_When_Declared()
    {
        var repositoryRoot = FindRepositoryRoot();
        var readRepositories = GetReadRepositories();
        var violations = readRepositories
            .SelectMany(repository => GetLocationViolations(
                repositoryRoot,
                repository.Type))
            .ToArray();

        Assert.NotEmpty(readRepositories);
        Assert.True(
            violations.Length == 0,
            $"Read repository location violations:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Read repositories should use primitive ids without aggregate roots when declared")]
    public void ReadRepositories_Should_UsePrimitiveIdsWithoutAggregateRoots_When_Declared()
    {
        var readRepositories = GetReadRepositories();
        var violations = readRepositories
            .SelectMany(GetBoundaryViolations)
            .ToArray();

        Assert.NotEmpty(readRepositories);
        Assert.True(
            violations.Length == 0,
            $"Read repository boundary type violations:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Read repositories should match feature names when declared")]
    public void ReadRepositories_Should_MatchFeatureNames_When_Declared()
    {
        var readRepositories = GetReadRepositories();
        var violations = readRepositories
            .Select(repository => new
            {
                repository.Type,
                ExpectedName =
                    "I" + GetFeatureName(repository.Type) +
                    ReadRepositorySuffix
            })
            .Where(repository => !string.Equals(
                repository.Type.Name,
                repository.ExpectedName,
                StringComparison.Ordinal))
            .Select(repository =>
                $"{repository.Type.FullName} must be named " +
                $"'{repository.ExpectedName}'.")
            .ToArray();

        Assert.NotEmpty(readRepositories);
        Assert.True(
            violations.Length == 0,
            $"Read repository interface naming violations:" +
            $"{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Read repository constructor parameters should follow type-based naming when read repository is injected")]
    public void ConstructorParameters_Should_FollowTypeBasedNaming_When_ReadRepositoryIsInjected()
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
                    .Where(parameter => GetClosedReadRepositoryContract(
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

    private static ReadRepository[] GetReadRepositories()
    {
        return
        [
            .. ArchitectureDefinition.ProductionAssemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsInterface)
                .Select(type => new
                {
                    Type = type,
                    Contract = GetClosedReadRepositoryContract(type)
                })
                .Where(repository => repository.Contract is not null)
                .Select(repository => new ReadRepository(
                    repository.Type,
                    repository.Contract!))
                .OrderBy(
                    repository => repository.Type.FullName,
                    StringComparer.Ordinal)
        ];
    }

    private static IEnumerable<string> GetLocationViolations(
        string repositoryRoot,
        Type readRepository)
    {
        var violations = new List<string>();
        var assemblyName = GetAssemblyName(readRepository);

        if (!assemblyName.EndsWith(
                ApplicationAssemblySuffix,
                StringComparison.Ordinal))
        {
            violations.Add(
                $"{readRepository.FullName} inherits IReadRepository<> and " +
                $"must be declared in Application.");
        }

        if (!HasNamespaceSegment(
                readRepository,
                AbstractionsNamespaceSegment))
        {
            violations.Add(
                $"{readRepository.FullName} must be declared in an " +
                $"'{AbstractionsNamespaceSegment}' namespace.");
        }

        var expectedSourceFilePath = GetExpectedSourceFilePath(
            repositoryRoot,
            readRepository);

        if (!File.Exists(expectedSourceFilePath))
        {
            violations.Add(
                $"{readRepository.FullName} must have a matching source file " +
                $"at '{Path.GetRelativePath(
                    repositoryRoot,
                    expectedSourceFilePath)}'.");
        }

        return violations;
    }

    private static IEnumerable<string> GetBoundaryViolations(
        ReadRepository readRepository)
    {
        var identifierType =
            readRepository.Contract.GetGenericArguments()[0];
        var violations = new List<string>();

        if (!IsPrimitiveValue(identifierType))
        {
            violations.Add(
                $"{readRepository.Type.FullName} must use a primitive read " +
                $"identifier, found '{identifierType.FullName}'.");
        }

        violations.AddRange(readRepository.Type
            .GetMethods()
            .Where(MethodContainsAggregateRoot)
            .Select(method =>
                $"{readRepository.Type.FullName}.{method.Name} must not use " +
                $"an aggregate root in parameters or return types."));

        return violations;
    }

    private static Type? GetClosedReadRepositoryContract(Type type)
    {
        return type
            .GetInterfaces()
            .Append(type)
            .FirstOrDefault(candidate =>
                candidate.IsGenericType &&
                candidate.GetGenericTypeDefinition() ==
                typeof(IReadRepository<>));
    }

    private static bool MethodContainsAggregateRoot(MethodInfo method)
    {
        return ContainsAggregateRoot(
                   method.ReturnType,
                   new HashSet<Type>()) ||
               method.GetParameters().Any(parameter =>
                   ContainsAggregateRoot(
                       parameter.ParameterType,
                       new HashSet<Type>()));
    }

    private static bool ContainsAggregateRoot(
        Type type,
        ISet<Type> visitedTypes)
    {
        if (!visitedTypes.Add(type))
        {
            return false;
        }

        if (typeof(IAggregateRoot).IsAssignableFrom(type))
        {
            return true;
        }

        if (type.HasElementType)
        {
            return ContainsAggregateRoot(
                type.GetElementType()
                    ?? throw new InvalidOperationException(
                        $"Could not determine element type for '{type}'."),
                visitedTypes);
        }

        return type.IsGenericType &&
               type.GetGenericArguments().Any(argument =>
                   ContainsAggregateRoot(argument, visitedTypes));
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

    private static string GetFeatureName(Type readRepository)
    {
        var repositoryNamespace = readRepository.Namespace
            ?? throw new InvalidOperationException(
                $"Read repository interface '{readRepository.FullName}' does " +
                $"not have a namespace.");
        var namespaceSegments = repositoryNamespace.Split('.');
        var abstractionsIndex = Array.LastIndexOf(
            namespaceSegments,
            AbstractionsNamespaceSegment);

        if (abstractionsIndex < 1)
        {
            throw new InvalidOperationException(
                $"Read repository interface '{readRepository.FullName}' must " +
                $"be declared in an '*.{{Feature}}." +
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
        Type readRepository)
    {
        var assemblyName = GetAssemblyName(readRepository);
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
        var repositoryNamespace = readRepository.Namespace
            ?? throw new InvalidOperationException(
                $"Read repository interface '{readRepository.FullName}' does " +
                $"not have a namespace.");
        var namespacePrefix = assemblyName + ".";
        var relativeNamespace = repositoryNamespace.StartsWith(
            namespacePrefix,
            StringComparison.Ordinal)
            ? repositoryNamespace[namespacePrefix.Length..]
            : throw new InvalidOperationException(
                $"Read repository interface '{readRepository.FullName}' " +
                $"namespace must start with '{namespacePrefix}'.");

        return Path.Combine(
            repositoryRoot,
            SourceDirectoryName,
            moduleDirectoryName,
            assemblyName,
            relativeNamespace.Replace(
                '.',
                Path.DirectorySeparatorChar),
            $"{readRepository.Name.Split('`')[0]}.cs");
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

    private sealed record ReadRepository(
        Type Type,
        Type Contract);

    private sealed record ConstructorParameter(
        Type DeclaringType,
        ParameterInfo Parameter);
}
