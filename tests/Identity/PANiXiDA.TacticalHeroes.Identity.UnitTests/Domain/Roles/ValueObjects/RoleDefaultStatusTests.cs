using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Roles.ValueObjects;

public sealed class RoleDefaultStatusTests
{
    [Theory(DisplayName = "Role default status should preserve the flag when boolean is provided")]
    [InlineData(false)]
    [InlineData(true)]
    public void From_Should_PreserveFlag_When_BooleanIsProvided(bool isDefault)
    {
        var status = RoleDefaultStatus.From(isDefault);

        status.IsDefault.ShouldBe(isDefault);
    }

    [Theory(DisplayName = "Role default status should format its flag when converted to string")]
    [InlineData(false, "RoleDefaultStatus { IsDefault = False }")]
    [InlineData(true, "RoleDefaultStatus { IsDefault = True }")]
    public void ToString_Should_FormatFlag_When_ConvertedToString(bool isDefault, string expected)
    {
        var status = RoleDefaultStatus.From(isDefault);

        var result = status.ToString();

        result.ShouldBe(expected);
    }
}
