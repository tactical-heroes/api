using PANiXiDA.TacticalHeroes.Identity.Application.Users.Block;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.Block;

public sealed class BlockUserCommandValidatorTests
{
    [Fact(DisplayName = "Block user validator should reject an empty user id")]
    public void Validate_Should_ReturnError_When_UserIdIsEmpty()
    {
        var validator = new BlockUserCommandValidator();

        var result = validator.Validate(new BlockUserCommand(Guid.Empty));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(BlockUserCommand.Id));
    }
}
