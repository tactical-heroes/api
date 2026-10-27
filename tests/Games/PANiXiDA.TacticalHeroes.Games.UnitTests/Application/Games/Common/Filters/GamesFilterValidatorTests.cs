using PANiXiDA.TacticalHeroes.Games.Application.Games.Common.Filters;

namespace PANiXiDA.TacticalHeroes.Games.UnitTests.Application.Games.Common.Filters;

public sealed class GamesFilterValidatorTests
{
    [Fact(DisplayName = "Games filter should accept empty input when validated")]
    public async Task ValidateAsync_Should_AcceptEmptyInput_When_Validated()
    {
        var validator = new GamesFilterValidator();
        var filter = new GamesFilter();

        var result = await validator.ValidateAsync(filter, TestContext.Current.CancellationToken);

        result.IsValid.ShouldBeTrue();
    }
}
