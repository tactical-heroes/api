using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.ConfirmMatch;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Application.MatchSearches.ConfirmMatch;

public sealed class ConfirmMatchCommandValidatorTests
{
    [Theory(DisplayName = "Validator should validate pair ids when command is created")]
    [InlineData("Valid", true)]
    [InlineData("EmptyFirst", false)]
    [InlineData("EmptySecond", false)]
    [InlineData("Same", false)]
    public void Validate_Should_ValidatePairIds_When_CommandIsCreated(string scenario, bool valid)
    {
        var id = Guid.CreateVersion7();
        var command = new ConfirmMatchCommand(
            scenario == "EmptyFirst" ? Guid.Empty : id,
            scenario == "EmptySecond" ? Guid.Empty : scenario == "Same" ? id : Guid.CreateVersion7());
        var validator = new ConfirmMatchCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.ShouldBe(valid);
    }
}
