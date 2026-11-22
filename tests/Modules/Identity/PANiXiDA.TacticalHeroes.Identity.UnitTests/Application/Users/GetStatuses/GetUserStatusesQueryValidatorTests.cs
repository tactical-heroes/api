using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetStatuses;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.GetStatuses;

public sealed class GetUserStatusesQueryValidatorTests
{
    [Fact(DisplayName = "User statuses validator should accept a query when query is valid")]
    public void Validate_Should_ReturnSuccess_When_QueryIsValid()
    {
        var validator = new GetUserStatusesQueryValidator();

        var result = validator.Validate(new GetUserStatusesQuery());

        result.IsValid.ShouldBeTrue();
    }
}
