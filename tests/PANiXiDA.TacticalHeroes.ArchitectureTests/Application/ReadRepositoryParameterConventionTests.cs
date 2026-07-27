using System.Reflection;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Application;

public sealed class ReadRepositoryParameterConventionTests
{
    private const string ApplicationAssemblySuffix = ".Application";
    private const string ReadRepositorySuffix = "ReadRepository";

    [Fact(DisplayName = "Read repository constructor parameters should follow type-based naming")]
    public void ConstructorParameters_Should_FollowTypeBasedNaming_When_ReadRepositoryIsInjected()
    {
        var readRepositoryParameters = ArchitectureDefinition.ProductionAssemblies
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
                    .Where(parameter => IsReadRepository(parameter.ParameterType))
                    .Select(parameter => new
                    {
                        DeclaringType = type,
                        Parameter = parameter
                    })))
            .OrderBy(candidate => candidate.DeclaringType.FullName, StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(readRepositoryParameters);

        var violations = readRepositoryParameters
            .Select(candidate => new
            {
                candidate.DeclaringType,
                candidate.Parameter,
                ExpectedName = GetExpectedParameterName(candidate.Parameter.ParameterType)
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

    private static bool IsReadRepository(Type parameterType)
    {
        var typeName = RemoveGenericArity(parameterType.Name);

        return parameterType.IsInterface &&
               typeName.StartsWith('I') &&
               typeName.EndsWith(ReadRepositorySuffix, StringComparison.Ordinal);
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
        var genericAritySeparatorIndex = typeName.IndexOf('`', StringComparison.Ordinal);

        return genericAritySeparatorIndex < 0
            ? typeName
            : typeName[..genericAritySeparatorIndex];
    }
}
