using PANiXiDA.TacticalHeroes.Games.Domain.Games.Entities.GameParticipants.Enumerations;

namespace PANiXiDA.TacticalHeroes.Games.UnitTests.Domain.Games.Entities.GameParticipants.Enumerations;

public sealed class GameSideTests
{
    [Fact(DisplayName = "Game side should preserve identifier order when called")]
    public void GetAll_Should_ReturnValuesInIdOrder_When_Called()
    {
        var values = GameSide.GetAll();

        values.ShouldBe([GameSide.First, GameSide.Second]);
    }

    [Theory(DisplayName = "Game side should resolve known identifiers when id is known")]
    [InlineData(1, "First")]
    [InlineData(2, "Second")]
    public void FromId_Should_ReturnValue_When_IdIsKnown(int id, string name)
    {
        var value = GameSide.FromId(id);

        value.Name.ShouldBe(name);
    }

    [Fact(DisplayName = "Game side should reject unknown identifiers when id is unknown")]
    public void FromId_Should_Throw_When_IdIsUnknown()
    {
        Action action = () => GameSide.FromId(0);

        action.ShouldThrow<InvalidOperationException>();
    }

    [Fact(DisplayName = "Game side should resolve trimmed names when name is known")]
    public void FromName_Should_ReturnValue_When_NameIsKnown()
    {
        var value = GameSide.FromName("  First  ");

        value.ShouldBe(GameSide.First);
    }

    [Fact(DisplayName = "Game side should reject unknown names when name is unknown")]
    public void FromName_Should_Throw_When_NameIsUnknown()
    {
        Action action = () => GameSide.FromName("Unknown");

        action.ShouldThrow<InvalidOperationException>();
    }

    [Theory(DisplayName = "Game side lookup should report matches when id is provided")]
    [InlineData(1, true)]
    [InlineData(0, false)]
    public void TryFromId_Should_ReportMatch_When_IdIsProvided(int id, bool expected)
    {
        var found = GameSide.TryFromId(id, out var value);

        found.ShouldBe(expected);
        (value is not null).ShouldBe(expected);
    }

    [Theory(DisplayName = "Game side lookup should report matches when name is provided")]
    [InlineData("First", true)]
    [InlineData("Unknown", false)]
    [InlineData("", false)]
    public void TryFromName_Should_ReportMatch_When_NameIsProvided(string name, bool expected)
    {
        var found = GameSide.TryFromName(name, out var value);

        found.ShouldBe(expected);
        (value is not null).ShouldBe(expected);
    }

    [Fact(DisplayName = "Game side should normalize known names when name is known")]
    public void Create_Should_ReturnValue_When_NameIsKnown()
    {
        var result = GameSide.Create("  First  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(GameSide.First);
    }

    [Theory(DisplayName = "Game side should reject missing names when name is empty")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_Should_ReturnValidationFailure_When_NameIsEmpty(string? name)
    {
        var result = GameSide.Create(name!);

        result.ShouldHaveSingleError(ErrorType.Validation, "Game side is required.")
            .ShouldHaveField(nameof(GameSide));
    }

    [Fact(DisplayName = "Game side should reject unknown names when name is unknown")]
    public void Create_Should_ReturnValidationFailure_When_NameIsUnknown()
    {
        var result = GameSide.Create("Unknown");

        result.ShouldHaveSingleError(ErrorType.Validation, "Game side 'Unknown' is invalid.")
            .ShouldHaveField(nameof(GameSide));
    }

    [Theory(DisplayName = "Game side should provide Russian display names when name is known")]
    [InlineData("First", "Первая сторона")]
    [InlineData("Second", "Вторая сторона")]
    public void Create_Should_ProvideRussianDisplayNames_When_NameIsKnown(string name, string displayName)
    {
        var value = GameSide.Create(name).Value;

        value.DisplayName.ShouldBe(displayName);
        value.Name.Length.ShouldBeLessThanOrEqualTo(GameSide.MaxLength);
    }
}
