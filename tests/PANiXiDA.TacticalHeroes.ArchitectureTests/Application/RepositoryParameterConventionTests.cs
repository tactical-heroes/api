using System.Reflection;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Application;

public sealed class RepositoryParameterConventionTests
{
    private const string ApplicationAssemblySuffix = ".Application";
    private const string ReadRepositorySuffix = "ReadRepository";
    private const string AggregateRepositoryInterfaceName =
        "PANiXiDA.Core.Domain.Abstractions.IRepository`2";

    [Fact(DisplayName = "Aggregate repository constructor parameters should follow type-based naming")]
    public void ConstructorParameters_Should_FollowTypeBasedNaming_When_AggregateRepositoryIsInjected()
    {
        AssertConstructorParametersFollowTypeBasedNaming(IsAggregateRepository);
    }

    [Fact(DisplayName = "Read repository constructor parameters should follow type-based naming")]
    public void ConstructorParameters_Should_FollowTypeBasedNaming_When_ReadRepositoryIsInjected()
    {
        AssertConstructorParametersFollowTypeBasedNaming(IsReadRepository);
    }

    private static void AssertConstructorParametersFollowTypeBasedNaming(
        Func<Type, bool> isRepository)
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
                    .Where(parameter => isRepository(parameter.ParameterType))
                    .Select(parameter => new ConstructorParameter(type, parameter))))
            .OrderBy(candidate => candidate.DeclaringType.FullName, StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(repositoryParameters);

        var violations = repositoryParameters
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

    private static bool IsAggregateRepository(Type parameterType)
    {
        return parameterType.IsInterface &&
               parameterType
                   .GetInterfaces()
                   .Append(parameterType)
                   .Any(candidate =>
                       candidate.IsGenericType &&
                       string.Equals(
                           candidate.GetGenericTypeDefinition().FullName,
                           AggregateRepositoryInterfaceName,
                           StringComparison.Ordinal));
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

    private sealed record ConstructorParameter(
        Type DeclaringType,
        ParameterInfo Parameter);
}
