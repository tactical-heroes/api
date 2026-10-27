using PANiXiDA.TacticalHeroes.Builds.Application.Builds.Common.Filters;

namespace PANiXiDA.TacticalHeroes.Builds.UnitTests.Application.Builds.Common.Filters;

public sealed class BuildsFilterValidatorTests
{
    [Fact(DisplayName = "Builds filter should accept empty input when validated")]
    public async Task ValidateAsync_Should_AcceptEmptyInput_When_Validated()
    {
        var validator = new BuildsFilterValidator();
        var filter = new BuildsFilter();

        var result = await validator.ValidateAsync(filter, TestContext.Current.CancellationToken);

        result.IsValid.ShouldBeTrue();
    }
}
