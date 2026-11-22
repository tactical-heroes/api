using PANiXiDA.TacticalHeroes.Builds.Application.Builds.GetInternalBuild;

namespace PANiXiDA.TacticalHeroes.Builds.UnitTests.Application.Builds.GetInternalBuild;

public sealed class GetInternalBuildQueryValidatorTests
{
    [Fact(DisplayName = "Validator should accept lookup when build id is valid")]
    public void Validate_Should_AcceptLookup_When_BuildIdIsValid()
    {
        var validator = new GetInternalBuildQueryValidator();

        var result = validator.Validate(new GetInternalBuildQuery(Guid.CreateVersion7()));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Validator should reject lookup when build id is empty")]
    public void Validate_Should_RejectLookup_When_BuildIdIsEmpty()
    {
        var validator = new GetInternalBuildQueryValidator();

        var result = validator.Validate(new GetInternalBuildQuery(Guid.Empty));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(GetInternalBuildQuery.Id));
    }
}
