using System.Reflection;
using System.Runtime.CompilerServices;

using FluentValidation;
using FluentValidation.Validators;

using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Messaging.Mediator.Handlers;
using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.Core.Application.Querying;
using PANiXiDA.Core.Application.Querying.Pagination;
using PANiXiDA.Core.Application.Querying.Sorting;
using PANiXiDA.Core.ResultPattern;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Global;

public sealed class PaginationConventionTests
{
    [Fact(DisplayName = "Read repositories should pair pagination parameters and results when methods are declared")]
    public void ReadRepositories_Should_PairPaginationParametersAndResults_When_MethodsAreDeclared()
    {
        var methods = GetMethods().Where(method => !method.IsStatic &&
            (method.IsPublic || method.IsPrivate && method.IsFinal) &&
            method.DeclaringType!.GetInterfaces().Append(method.DeclaringType).Any(type =>
                type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadRepository<>))).ToArray();
        var violations = methods.Where(method =>
                method.GetParameters().Any(parameter => parameter.ParameterType == typeof(PaginationParameters)) !=
                IsPaginationResult(method.ReturnType))
            .Select(method => $"{method.DeclaringType?.FullName}.{method.Name}: PaginationParameters and PaginationResult<T> must occur together.")
            .ToArray();

        Assert.NotEmpty(methods);
        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Queries should pair pagination parameters and results when declared")]
    public void Queries_Should_PairPaginationParametersAndResults_When_Declared()
    {
        var queries = GetQueries().ToArray();
        var violations = queries.SelectMany(query => query.GetInterfaces()
                .Where(contract => contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IQuery<>))
                .Where(contract => HasPaginationParameters(query) != IsPaginationResult(contract.GetGenericArguments()[0]))
                .Select(contract => $"{query.FullName}: PaginationParameters and {contract} must agree on pagination."))
            .ToArray();

        Assert.NotEmpty(queries);
        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Query handlers should pair query pagination parameters and results when declared")]
    public void QueryHandlers_Should_PairQueryPaginationParametersAndResults_When_Declared()
    {
        var handlers = ArchitectureDefinition.ProductionAssemblies.SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .SelectMany(type => type.GetInterfaces()
                .Where(contract => contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))
                .Select(contract => new { Type = type, Contract = contract })).ToArray();
        var violations = handlers.Where(handler =>
                HasPaginationParameters(handler.Contract.GetGenericArguments()[0]) !=
                IsPaginationResult(handler.Contract.GetGenericArguments()[1]))
            .Select(handler => $"{handler.Type.FullName}: query PaginationParameters and handler PaginationResult<T> must occur together.")
            .ToArray();

        Assert.NotEmpty(handlers);
        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    private static bool HasPaginationParameters(Type type)
    {
        return type.GetProperties().Any(property => property.PropertyType == typeof(PaginationParameters));
    }

    private static bool IsPaginationResult(Type type)
    {
        if (!type.IsGenericType)
        {
            return false;
        }

        var definition = type.GetGenericTypeDefinition();
        return definition == typeof(PaginationResult<>) ||
            ((definition == typeof(Task<>) || definition == typeof(ValueTask<>) || definition == typeof(Result<>)) &&
                IsPaginationResult(type.GetGenericArguments()[0]));
    }

    [Theory(DisplayName = "Query validators should attach matching child validators when querying parameters are present")]
    [InlineData(typeof(PaginationParameters))]
    [InlineData(typeof(SortingParameters))]
    public void QueryValidators_Should_AttachMatchingChildValidators_When_QueryingParametersArePresent(Type parameterType)
    {
        var queries = GetQueries()
            .Where(type => type.GetProperties().Any(property => property.PropertyType == parameterType))
            .ToArray();
        var violations = queries
            .SelectMany(query => GetChildValidatorViolations(query, parameterType))
            .ToArray();

        Assert.NotEmpty(queries);
        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    [Theory(DisplayName = "Query properties should use type-based names when querying parameters are present")]
    [InlineData(typeof(PaginationParameters))]
    [InlineData(typeof(SortingParameters))]
    public void QueryProperties_Should_UseTypeBasedNames_When_QueryingParametersArePresent(Type parameterType)
    {
        var properties = GetQueries()
            .SelectMany(type => type.GetProperties())
            .Where(property => property.PropertyType == parameterType)
            .ToArray();
        var violations = properties
            .Where(property => property.Name != parameterType.Name)
            .Select(property =>
                $"{property.DeclaringType?.FullName}.{property.Name}: " +
                $"properties of type {parameterType.Name} must be named '{parameterType.Name}'.")
            .ToArray();

        Assert.NotEmpty(properties);
        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

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
        var queries = GetQueries()
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

    private static IEnumerable<string> GetChildValidatorViolations(Type query, Type parameterType)
    {
        var expectedValidator = typeof(PaginationParametersValidator);
        if (parameterType == typeof(SortingParameters))
        {
            var readModels = query.GetInterfaces()
                .Where(contract => contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IQuery<>))
                .SelectMany(contract => GetReadModels(contract.GetGenericArguments()[0]))
                .Distinct()
                .ToArray();
            if (readModels.Length != 1)
            {
                yield return $"{query.FullName}: sorting requires exactly one result read model.";
                yield break;
            }

            var readModel = readModels[0];
            var sortingValidator = readModel.Assembly.GetType($"{readModel.FullName}SortingValidator");
            if (sortingValidator is null || !typeof(SortingParametersValidator).IsAssignableFrom(sortingValidator))
            {
                yield return $"{query.FullName}: missing {readModel.FullName}SortingValidator.";
                yield break;
            }

            expectedValidator = sortingValidator;
        }

        var contract = typeof(IValidator<>).MakeGenericType(query);
        var validators = query.Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && contract.IsAssignableFrom(type))
            .ToArray();
        if (validators.Length == 0)
        {
            yield return $"{query.FullName}: missing IValidator<{query.Name}>.";
        }

        foreach (var validatorType in validators)
        {
            var validator = (IValidator)Activator.CreateInstance(validatorType)!;
            var descriptor = validator.CreateDescriptor();
            foreach (var property in query.GetProperties().Where(property => property.PropertyType == parameterType))
            {
                if (!descriptor.GetValidatorsForMember(property.Name).Any(component =>
                        component.Validator is IChildValidatorAdaptor adaptor && adaptor.ValidatorType == expectedValidator))
                {
                    yield return $"{validatorType.FullName}: {property.Name} must use {expectedValidator.FullName}.";
                }
            }
        }
    }

    private static IEnumerable<Type> GetReadModels(Type type)
    {
        return typeof(IReadModel).IsAssignableFrom(type)
            ? [type]
            : type.GetGenericArguments().SelectMany(GetReadModels);
    }

    private static IEnumerable<Type> GetQueries()
    {
        return ArchitectureDefinition.ProductionAssemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.GetInterfaces().Any(contract =>
                contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IQuery<>)));
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
