using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Domain.Heroes;

public sealed class HeroIdTests
{
    [Fact(DisplayName = "Hero id should create a version 7 identifier when invoked")]
    public void New_Should_CreateVersion7Guid_When_Invoked()
    {
        var id = HeroId.New();

        id.Value.ShouldNotBe(Guid.Empty);
        id.Value.Version.ShouldBe(7);
        id.ToString().ShouldBe(id.Value.ToString());
    }

    [Fact(DisplayName = "Hero id should reject an empty identifier when id is empty")]
    public void Create_Should_ReturnValidationFailure_When_IdIsEmpty()
    {
        var result = HeroId.Create(Guid.Empty);

        result.ShouldHaveSingleError(
            ErrorType.Validation,
            "Hero id cannot be empty.");
    }

    [Fact(DisplayName = "Hero id should return its value when converted to string")]
    public void ToString_Should_ReturnValue_When_ConvertedToString()
    {
        var value = Guid.CreateVersion7();
        var id = HeroId.Create(value).Value;

        var result = id.ToString();

        result.ShouldBe(value.ToString());
    }
}
