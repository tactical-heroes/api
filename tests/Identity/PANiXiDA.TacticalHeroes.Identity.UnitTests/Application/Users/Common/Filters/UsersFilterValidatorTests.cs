using PANiXiDA.TacticalHeroes.Identity.Application.Users.Common.Filters;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.Common.Filters;

public sealed class UsersFilterValidatorTests
{
    [Theory(DisplayName = "Validate should accept filter when email is omitted or partial")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("hero")]
    [InlineData(" hero ")]
    public void Validate_Should_AcceptFilter_When_EmailIsOmittedOrPartial(string? email)
    {
        var validator = new UsersFilterValidator();

        var result = validator.Validate(new UsersFilter(email));

        result.IsValid.ShouldBeTrue();
    }
}
