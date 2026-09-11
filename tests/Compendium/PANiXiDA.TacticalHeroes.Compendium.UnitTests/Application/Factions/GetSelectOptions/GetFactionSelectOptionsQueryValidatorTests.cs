using PANiXiDA.Core.Application.Querying.Limiting;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Common.Filters;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetSelectOptions;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Factions.GetSelectOptions;

public sealed class GetFactionSelectOptionsQueryValidatorTests
{
    [Theory(DisplayName = "Validate should reject limit when limit is outside bounds")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(201)]
    public void Validate_Should_RejectLimit_When_LimitIsOutsideBounds(int limit)
    {
        var validator = new GetFactionSelectOptionsQueryValidator();

        var result = validator.Validate(new GetFactionSelectOptionsQuery(new FactionsFilter(), new LimitParameters(limit)));

        result.Errors.ShouldContain(error => error.PropertyName == "Limit.Limit");
    }

    [Fact(DisplayName = "Validate should reject filter when filter is null")]
    public void Validate_Should_RejectFilter_When_FilterIsNull()
    {
        var validator = new GetFactionSelectOptionsQueryValidator();

        var result = validator.Validate(new GetFactionSelectOptionsQuery(null!, new LimitParameters(20)));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(GetFactionSelectOptionsQuery.Filter));
    }

    [Fact(DisplayName = "Validate should apply filter rules when search is invalid")]
    public void Validate_Should_ApplyFilterRules_When_SearchIsInvalid()
    {
        var validator = new GetFactionSelectOptionsQueryValidator();

        var result = validator.Validate(new GetFactionSelectOptionsQuery(new FactionsFilter(" no "), new LimitParameters(20)));

        result.Errors.ShouldContain(error => error.PropertyName == "Filter.Search");
    }

    [Theory(DisplayName = "Validate should accept query when parameters are valid")]
    [InlineData(null, 1)]
    [InlineData("nor", 200)]
    [InlineData(" north ", 20)]
    public void Validate_Should_AcceptQuery_When_ParametersAreValid(string? search, int limit)
    {
        var validator = new GetFactionSelectOptionsQueryValidator();

        var result = validator.Validate(new GetFactionSelectOptionsQuery(new FactionsFilter(search), new LimitParameters(limit)));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Validate should reject limit when limit parameters are null")]
    public void Validate_Should_RejectLimit_When_LimitParametersAreNull()
    {
        var validator = new GetFactionSelectOptionsQueryValidator();

        var result = validator.Validate(new GetFactionSelectOptionsQuery(new FactionsFilter(), null!));

        result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe("Limit");
    }
}
