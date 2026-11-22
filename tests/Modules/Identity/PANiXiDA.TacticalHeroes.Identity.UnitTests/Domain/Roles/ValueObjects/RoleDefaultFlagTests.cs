using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Roles.ValueObjects;

public sealed class RoleDefaultFlagTests
{
    [Theory(DisplayName = "Role default flag should preserve the value when boolean is provided")]
    [InlineData(false)]
    [InlineData(true)]
    public void Create_Should_PreserveValue_When_BooleanIsProvided(bool value)
    {
        var result = RoleDefaultFlag.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
    }

    [Theory(DisplayName = "Role default flag should format its value when converted to string")]
    [InlineData(false, "RoleDefaultFlag { Value = False }")]
    [InlineData(true, "RoleDefaultFlag { Value = True }")]
    public void ToString_Should_FormatValue_When_ConvertedToString(bool value, string expected)
    {
        var flag = RoleDefaultFlag.Create(value).Value;

        var result = flag.ToString();

        result.ShouldBe(expected);
    }
}
