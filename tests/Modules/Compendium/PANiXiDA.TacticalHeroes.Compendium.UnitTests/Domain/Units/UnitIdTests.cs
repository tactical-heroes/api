using PANiXiDA.TacticalHeroes.Compendium.Domain.Units;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Domain.Units;

public sealed class UnitIdTests
{
    [Fact(DisplayName = "Unit id should create a version 7 identifier when invoked")]
    public void New_Should_CreateVersion7Guid_When_Invoked()
    {
        var id = UnitId.New();

        id.Value.ShouldNotBe(Guid.Empty);
        id.Value.Version.ShouldBe(7);
        id.ToString().ShouldBe(id.Value.ToString());
    }

    [Fact(DisplayName = "Unit id should reject an empty identifier when id is empty")]
    public void Create_Should_ReturnValidationFailure_When_IdIsEmpty()
    {
        var result = UnitId.Create(Guid.Empty);

        result.ShouldHaveSingleError(
            ErrorType.Validation,
            "Unit id cannot be empty.");
    }

    [Fact(DisplayName = "Unit id should return its value when converted to string")]
    public void ToString_Should_ReturnValue_When_ConvertedToString()
    {
        var value = Guid.CreateVersion7();
        var id = UnitId.Create(value).Value;

        var result = id.ToString();

        result.ShouldBe(value.ToString());
    }
}
