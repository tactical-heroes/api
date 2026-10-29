using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Cancel;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Application.MatchSearches.Cancel;

public sealed class CancelMatchSearchCommandValidatorTests
{
    [Theory(DisplayName = "Validator should validate search identifier when id validity varies")]
    [InlineData(false)]
    [InlineData(true)]
    public void Validate_Should_ValidateSearchIdentifier_When_IdValidityVaries(bool valid)
    {
        var command = new CancelMatchSearchCommand(valid ? Guid.CreateVersion7() : Guid.Empty);

        var result = new CancelMatchSearchCommandValidator().Validate(command);

        result.IsValid.ShouldBe(valid);
        if (!valid)
        {
            result.Errors.ShouldContain(error => error.PropertyName == nameof(command.Id));
        }
    }
}
