using PANiXiDA.TacticalHeroes.Identity.Application.Users;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users;

public sealed class UserMapperTests
{
    [Fact(DisplayName = "User mapper should collect invalid identity fields when identity is invalid")]
    public void ToDomain_Should_ReturnValidationFailures_When_IdentityIsInvalid()
    {
        var result = UserMapper.ToDomain(Guid.Empty, "invalid-email", false, [], []);

        result.IsFailure.ShouldBeTrue();
        result.Errors.Count.ShouldBe(2);
    }

    [Fact(DisplayName = "User mapper should reject an empty role id when role id is empty")]
    public void ToDomain_Should_ReturnValidationFailure_When_RoleIdIsEmpty()
    {
        var result = UserMapper.ToDomain(Guid.CreateVersion7(), "hero@example.com", false, [Guid.Empty], []);

        result.ShouldHaveSingleError(ErrorType.Validation, "Role id cannot be empty.");
    }

    [Fact(DisplayName = "User mapper should reject invalid claims when claim is invalid")]
    public void ToDomain_Should_ReturnValidationFailures_When_ClaimIsInvalid()
    {
        var result = UserMapper.ToDomain(Guid.CreateVersion7(), "hero@example.com", false, [], [("", "")]);

        result.IsFailure.ShouldBeTrue();
        result.Errors.Count.ShouldBe(2);
    }

    [Fact(DisplayName = "User mapper should preserve confirmation and deduplicate authorization state when values are valid")]
    public void ToDomain_Should_RestoreAuthorizationState_When_ValuesAreValid()
    {
        var roleId = Guid.CreateVersion7();

        var result = UserMapper.ToDomain(
            Guid.CreateVersion7(), " HERO@Example.com ", true,
            [roleId, roleId], [(" permission ", " heroes.read "), ("permission", "heroes.read")]);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Email.Value.ShouldBe("hero@example.com");
        result.Value.ConfirmationStatus.IsConfirmed.ShouldBeTrue();
        result.Value.RoleIds.ShouldHaveSingleItem().Value.ShouldBe(roleId);
        result.Value.Claims.ShouldHaveSingleItem().Value.Value.ShouldBe("heroes.read");
        result.Value.GetDomainEvents().ShouldBeEmpty();
    }
}
