using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.Entities.BuildHeroes;

namespace PANiXiDA.TacticalHeroes.Builds.UnitTests.Domain.Builds.Entities.BuildHeroes;

public sealed class BuildHeroIdTests
{
    [Fact(DisplayName = "BuildHero id should create a version 7 identifier when invoked")]
    public void New_Should_CreateVersion7Guid_When_Invoked()
    {
        var id = BuildHeroId.New();

        id.Value.ShouldNotBe(Guid.Empty);
        id.Value.Version.ShouldBe(7);
        id.ToString().ShouldBe(id.Value.ToString());
    }

    [Fact(DisplayName = "BuildHero id should reject an empty identifier when id is empty")]
    public void Create_Should_ReturnValidationFailure_When_IdIsEmpty()
    {
        var result = BuildHeroId.Create(Guid.Empty);

        result.ShouldHaveSingleError(
            ErrorType.Validation,
            "BuildHero id cannot be empty.");
    }

    [Fact(DisplayName = "BuildHero id should return its value when converted to string")]
    public void ToString_Should_ReturnValue_When_ConvertedToString()
    {
        var value = Guid.CreateVersion7();
        var id = BuildHeroId.Create(value).Value;

        var result = id.ToString();

        result.ShouldBe(value.ToString());
    }
}
