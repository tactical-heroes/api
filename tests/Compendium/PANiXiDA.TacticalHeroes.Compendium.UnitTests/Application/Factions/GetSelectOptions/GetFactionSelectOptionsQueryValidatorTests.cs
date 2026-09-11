using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetSelectOptions;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Factions.GetSelectOptions;

public sealed class GetFactionSelectOptionsQueryValidatorTests
{
    [Theory(DisplayName = "Validate should reject limit when limit is outside bounds")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void Validate_Should_RejectLimit_When_LimitIsOutsideBounds(int limit)
    {
        var validator = new GetFactionSelectOptionsQueryValidator();

        var result = validator.Validate(new GetFactionSelectOptionsQuery(null, limit));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(GetFactionSelectOptionsQuery.Limit));
    }

    [Fact(DisplayName = "Validate should reject search when search exceeds maximum length")]
    public void Validate_Should_RejectSearch_When_SearchExceedsMaximumLength()
    {
        var validator = new GetFactionSelectOptionsQueryValidator();

        var result = validator.Validate(new GetFactionSelectOptionsQuery(new string('a', 129), 20));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(GetFactionSelectOptionsQuery.Search));
    }

    [Theory(DisplayName = "Validate should accept query when parameters are valid")]
    [InlineData(null, 1)]
    [InlineData("", 100)]
    [InlineData(" north ", 20)]
    public void Validate_Should_AcceptQuery_When_ParametersAreValid(string? search, int limit)
    {
        var validator = new GetFactionSelectOptionsQueryValidator();

        var result = validator.Validate(new GetFactionSelectOptionsQuery(search, limit));

        result.IsValid.ShouldBeTrue();
    }
}
