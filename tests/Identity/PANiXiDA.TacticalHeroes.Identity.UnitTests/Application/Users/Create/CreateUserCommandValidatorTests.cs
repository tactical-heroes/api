using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Create;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.Create;

public sealed class CreateUserCommandValidatorTests
{
    [Fact(DisplayName = "Create user validator should accept valid input when command is valid")]
    public void Validate_Should_ReturnValidResult_When_CommandIsValid()
    {
        var validator = new CreateUserCommandValidator();

        var result = validator.Validate(
            new CreateUserCommand(
                "hero@example.com",
                "hero",
                "StrongPassword1!",
                true,
                [new Claim("permission", "compendium.heroes.read")],
                "Active"));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Create user validator should reject invalid input when command is invalid")]
    public void Validate_Should_ReturnErrors_When_CommandIsInvalid()
    {
        var validator = new CreateUserCommandValidator();

        var result = validator.Validate(
            new CreateUserCommand("invalid", "", "", false, [new Claim("", "")], "Deleted"));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(CreateUserCommand.Email));
        result.Errors.ShouldContain(error => error.PropertyName == nameof(CreateUserCommand.UserName));
        result.Errors.ShouldContain(error => error.PropertyName == nameof(CreateUserCommand.Password));
        result.Errors
            .Count(error =>
                error.PropertyName.StartsWith("Claims[0]", StringComparison.Ordinal))
            .ShouldBe(2);
        result.Errors.ShouldContain(error => error.PropertyName == nameof(CreateUserCommand.Status));
    }

    [Theory(DisplayName = "User validator should validate role identifiers when role ids are provided")]
    [InlineData(false, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    public void Validate_Should_CheckRoleIds_When_RoleIdsAreProvided(bool emptyId, bool duplicate, bool expectedValid)
    {
        var validator = new CreateUserCommandValidator();
        var id = emptyId ? Guid.Empty : Guid.CreateVersion7();
        Guid[] roleIds = duplicate ? [id, id] : [id];

        var result = validator.Validate(
            new CreateUserCommand(
                Email: "hero@example.com",
                UserName: "hero",
                Password: "StrongPassword1!",
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
