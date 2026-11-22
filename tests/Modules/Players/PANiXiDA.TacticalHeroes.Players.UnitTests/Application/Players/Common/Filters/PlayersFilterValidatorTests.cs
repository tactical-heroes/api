using PANiXiDA.TacticalHeroes.Modules.Players.Application.Players.Common.Filters;

namespace PANiXiDA.TacticalHeroes.Modules.Players.UnitTests.Application.Players.Common.Filters;

public sealed class PlayersFilterValidatorTests
{
    [Fact(DisplayName = "Players filter should accept empty input when validated")]
    public async Task ValidateAsync_Should_AcceptEmptyInput_When_Validated()
    {
        var validator = new PlayersFilterValidator();
        var filter = new PlayersFilter();

        var result = await validator.ValidateAsync(filter, TestContext.Current.CancellationToken);

        result.IsValid.ShouldBeTrue();
    }
}
