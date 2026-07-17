using System.Reflection;

using FluentValidation;

using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.TacticalHeroes.Identity.Application;
using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Register;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetList;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application;

public sealed class ApplicationValidationTests
{
    [Fact(DisplayName = "Requests should have a validator when they contain input")]
    public void Requests_Should_HaveValidator_When_TheyContainInput()
    {
        var applicationTypes = ApplicationAssembly.Instance.GetTypes();
        var requestTypes = applicationTypes
            .Where(IsRequest)
            .Where(type => type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Length > 0)
            .OrderBy(type => type.FullName)
            .ToArray();

        var requestsWithoutValidator = requestTypes
            .Where(requestType => !applicationTypes.Any(type => IsValidatorFor(type, requestType)))
            .Select(type => type.FullName)
            .ToArray();

        requestsWithoutValidator.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Register account validator should reject invalid input")]
    public void Validate_Should_RejectInvalidInput_When_RegisteringAccount()
    {
        var validator = new RegisterAccountCommandValidator();
        var command = new RegisterAccountCommand(Email: string.Empty, UserName: string.Empty, Password: string.Empty);

        var result = validator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.PropertyName == nameof(command.Email));
        result.Errors.ShouldContain(error => error.PropertyName == nameof(command.UserName));
        result.Errors.ShouldContain(error => error.PropertyName == nameof(command.Password));
    }

    [Fact(DisplayName = "Get roles validator should reject invalid pagination")]
    public void Validate_Should_RejectInvalidPagination_When_GettingRoles()
    {
        var validator = new GetRolesQueryValidator();
        var query = new GetRolesQuery(Pagination: new PaginationParameters(PageNumber: 0, PageSize: 0));

        var result = validator.Validate(query);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.PropertyName == "Pagination.PageNumber");
        result.Errors.ShouldContain(error => error.PropertyName == "Pagination.PageSize");
    }

    private static bool IsRequest(Type type)
    {
        return type is { IsAbstract: false, IsInterface: false } &&
            type.GetInterfaces().Any(interfaceType =>
            {
                if (!interfaceType.IsGenericType)
                {
                    return false;
                }

                var genericTypeDefinition = interfaceType.GetGenericTypeDefinition();

                return genericTypeDefinition == typeof(ICommand<>) ||
                    genericTypeDefinition == typeof(IQuery<>);
            });
    }

    private static bool IsValidatorFor(Type type, Type requestType)
    {
        return type is { IsAbstract: false, IsInterface: false } &&
            typeof(IValidator<>).MakeGenericType(requestType).IsAssignableFrom(type);
    }
}
