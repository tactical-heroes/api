using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Update;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.Update;

public sealed class UpdateUserCommandValidatorTests
{
    [Fact(DisplayName = "Update user validator should accept valid input when command is valid")]
    public void Validate_Should_ReturnValidResult_When_CommandIsValid()
    {
        var validator = new UpdateUserCommandValidator();

        var result = validator.Validate(
            new UpdateUserCommand(
                Guid.CreateVersion7(),
                "hero@example.com",
                "hero",
                true,
                [new Claim("permission", "compendium.heroes.update")],
                "Blocked"));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Update user validator should reject invalid input when command is invalid")]
    public void Validate_Should_ReturnErrors_When_CommandIsInvalid()
    {
        var validator = new UpdateUserCommandValidator();

        var result = validator.Validate(
            new UpdateUserCommand(
                Guid.Empty,
                "invalid",
                "",
                false,
                [new Claim("", "")],
                "Deleted"));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(UpdateUserCommand.Id));
        result.Errors.ShouldContain(error => error.PropertyName == nameof(UpdateUserCommand.Email));
        result.Errors.ShouldContain(error => error.PropertyName == nameof(UpdateUserCommand.UserName));
        result.Errors
            .Count(error =>
                error.PropertyName.StartsWith("Claims[0]", StringComparison.Ordinal))
            .ShouldBe(2);
        result.Errors.ShouldContain(error => error.PropertyName == nameof(UpdateUserCommand.Status));
    }

    [Theory(DisplayName = "User validator should validate role identifiers when role ids are provided")]
    [InlineData(false, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    public void Validate_Should_CheckRoleIds_When_RoleIdsAreProvided(bool emptyId, bool duplicate, bool expectedValid)
    {
        var validator = new UpdateUserCommandValidator();
        var id = emptyId ? Guid.Empty : Guid.CreateVersion7();
        Guid[] roleIds = duplicate ? [id, id] : [id];

        var result = validator.Validate(
            new UpdateUserCommand(
                Id: Guid.CreateVersion7(),
                Email: "hero@example.com",
                UserName: "hero",
                IsConfirmed: true,
                Claims: [],
                Status: "Active",
                RoleIds: roleIds));

        result.IsValid.ShouldBe(expectedValid);
        if (!expectedValid)
        {
            result.Errors.ShouldContain(error => error.PropertyName.StartsWith("RoleIds", StringComparison.Ordinal));
        }
    }
}
