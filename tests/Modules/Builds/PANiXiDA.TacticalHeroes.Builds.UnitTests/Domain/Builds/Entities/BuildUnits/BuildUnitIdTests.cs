using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.Entities.BuildUnits;

namespace PANiXiDA.TacticalHeroes.Builds.UnitTests.Domain.Builds.Entities.BuildUnits;

public sealed class BuildUnitIdTests
{
    [Fact(DisplayName = "BuildUnit id should create a version 7 identifier when invoked")]
    public void New_Should_CreateVersion7Guid_When_Invoked()
    {
        var id = BuildUnitId.New();

        id.Value.ShouldNotBe(Guid.Empty);
        id.Value.Version.ShouldBe(7);
        id.ToString().ShouldBe(id.Value.ToString());
    }

    [Fact(DisplayName = "BuildUnit id should reject an empty identifier when id is empty")]
    public void Create_Should_ReturnValidationFailure_When_IdIsEmpty()
    {
        var result = BuildUnitId.Create(Guid.Empty);

        result.ShouldHaveSingleError(
            ErrorType.Validation,
            "BuildUnit id cannot be empty.");
    }

    [Fact(DisplayName = "BuildUnit id should return its value when converted to string")]
    public void ToString_Should_ReturnValue_When_ConvertedToString()
    {
        var value = Guid.CreateVersion7();
        var id = BuildUnitId.Create(value).Value;

        var result = id.ToString();

        result.ShouldBe(value.ToString());
    }
}
