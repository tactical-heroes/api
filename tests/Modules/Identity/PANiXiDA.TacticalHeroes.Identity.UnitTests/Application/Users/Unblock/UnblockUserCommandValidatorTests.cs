using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Users.Unblock;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.UnitTests.Application.Users.Unblock;

public sealed class UnblockUserCommandValidatorTests
{
    [Fact(DisplayName = "Unblock user validator should reject an empty user id when user id is empty")]
    public void Validate_Should_ReturnError_When_UserIdIsEmpty()
    {
        var validator = new UnblockUserCommandValidator();

        var result = validator.Validate(new UnblockUserCommand(Guid.Empty));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(UnblockUserCommand.Id));
    }
}
