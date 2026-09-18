using System.Reflection;
using System.Runtime.CompilerServices;

using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Querying.Pagination;
using PANiXiDA.Core.Application.Querying.Sorting;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Global;

public sealed class PaginationConventionTests
{
    [Theory(DisplayName = "Querying parameters should use type-based names when declared on methods")]
    [InlineData(typeof(PaginationParameters), "paginationParameters")]
    [InlineData(typeof(SortingParameters), "sortingParameters")]
    public void QueryingParameters_Should_UseTypeBasedNames_When_DeclaredOnMethods(
        Type parameterType,
        string expectedName)
    {
        var parameters = GetMethods()
            .SelectMany(method => method.GetParameters())
            .Where(parameter => parameter.ParameterType == parameterType)
            .ToArray();
        var violations = parameters
            .Where(parameter => parameter.Name != expectedName)
            .Select(parameter =>
                $"{parameter.Member.DeclaringType?.FullName}.{parameter.Member.Name}: " +
                $"{parameterType.Name} parameter '{parameter.Name}' must be named '{expectedName}'.")
            .ToArray();

        Assert.NotEmpty(parameters);
        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Methods should accept sorting parameters when pagination parameters are present")]
    public void Methods_Should_AcceptSortingParameters_When_PaginationParametersArePresent()
    {
        var methods = GetMethods()
            .Where(method => method.GetParameters().Any(parameter =>
                parameter.ParameterType == typeof(PaginationParameters)))
            .ToArray();
        var violations = methods
            .Where(method => !method.GetParameters().Any(parameter =>
                parameter.ParameterType == typeof(SortingParameters)))
            .Select(method =>
                $"{method.DeclaringType?.FullName}.{method.Name}: " +
                "methods accepting PaginationParameters must also accept SortingParameters.")
            .ToArray();

        Assert.NotEmpty(methods);
        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Queries should contain sorting parameters when pagination parameters are present")]
    public void Queries_Should_ContainSortingParameters_When_PaginationParametersArePresent()
    {
        var queries = ArchitectureDefinition.ProductionAssemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.GetInterfaces().Any(contract =>
                contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IQuery<>)))
            .Where(type => type.GetProperties().Any(property =>
                property.PropertyType == typeof(PaginationParameters)))
            .ToArray();
        var violations = queries
            .Where(type => !type.GetProperties().Any(property =>
                property.PropertyType == typeof(SortingParameters)))
            .Select(type =>
                $"{type.FullName}: queries containing PaginationParameters " +
                "must also contain SortingParameters.")
            .ToArray();

        Assert.NotEmpty(queries);
        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    private static MethodInfo[] GetMethods()
    {
        return [.. ArchitectureDefinition.ProductionAssemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false))
            .SelectMany(type => type.GetMethods(
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            .Where(method => !method.IsSpecialName &&
                !method.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false))
            .OrderBy(method => method.DeclaringType?.FullName, StringComparer.Ordinal)
            .ThenBy(method => method.Name, StringComparer.Ordinal)];
    }
}
