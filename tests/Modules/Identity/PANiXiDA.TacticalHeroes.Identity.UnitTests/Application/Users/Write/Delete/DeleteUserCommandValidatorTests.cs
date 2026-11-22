using PANiXiDA.TacticalHeroes.Identity.Application.Users.Write.Delete;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.Write.Delete;

public sealed class DeleteUserCommandValidatorTests
{
    [Fact(DisplayName = "Delete user validator should reject an empty user id when user id is empty")]
    public void Validate_Should_ReturnError_When_UserIdIsEmpty()
    {
        var validator = new DeleteUserCommandValidator();

        var result = validator.Validate(new DeleteUserCommand(Guid.Empty));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(DeleteUserCommand.Id));
    }
}
