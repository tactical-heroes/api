using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Start;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Application.MatchSearches.Start;

public sealed class StartMatchSearchCommandValidatorTests
{
    [Theory(DisplayName = "Validator should accept request when mode is supported")]
    [InlineData("RankedDuel")]
    [InlineData("CasualDuel")]
    public void Validate_Should_AcceptRequest_When_ModeIsSupported(string mode)
    {
        var validator = new StartMatchSearchCommandValidator();

        var result = validator.Validate(new StartMatchSearchCommand(Guid.CreateVersion7(), mode));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Validator should reject request when build id is empty")]
    public void Validate_Should_RejectRequest_When_BuildIdIsEmpty()
    {
        var validator = new StartMatchSearchCommandValidator();

        var result = validator.Validate(new StartMatchSearchCommand(Guid.Empty, "RankedDuel"));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(StartMatchSearchCommand.BuildId));
    }

    [Theory(DisplayName = "Validator should reject request when mode is invalid")]
    [InlineData("")]
    [InlineData("Unknown")]
    public void Validate_Should_RejectRequest_When_ModeIsInvalid(string mode)
    {
        var validator = new StartMatchSearchCommandValidator();

        var result = validator.Validate(new StartMatchSearchCommand(Guid.CreateVersion7(), mode));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(StartMatchSearchCommand.Mode));
    }
}
