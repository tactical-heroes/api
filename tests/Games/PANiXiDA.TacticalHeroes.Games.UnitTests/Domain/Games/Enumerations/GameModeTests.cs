using PANiXiDA.TacticalHeroes.Games.Domain.Games.Enumerations;

namespace PANiXiDA.TacticalHeroes.Games.UnitTests.Domain.Games.Enumerations;

public sealed class GameModeTests
{
    [Fact(DisplayName = "Game mode should preserve identifier order when called")]
    public void GetAll_Should_ReturnValuesInIdOrder_When_Called()
    {
        var values = GameMode.GetAll();

        values.ShouldBe([GameMode.RankedDuel, GameMode.CasualDuel]);
    }

    [Theory(DisplayName = "Game mode should resolve known identifiers when id is known")]
    [InlineData(1, "RankedDuel")]
    [InlineData(2, "CasualDuel")]
    public void FromId_Should_ReturnValue_When_IdIsKnown(int id, string name)
    {
        var value = GameMode.FromId(id);

        value.Name.ShouldBe(name);
    }

    [Fact(DisplayName = "Game mode should reject unknown identifiers when id is unknown")]
    public void FromId_Should_Throw_When_IdIsUnknown()
    {
        Action action = () => GameMode.FromId(0);

        action.ShouldThrow<InvalidOperationException>();
    }

    [Fact(DisplayName = "Game mode should resolve trimmed names when name is known")]
    public void FromName_Should_ReturnValue_When_NameIsKnown()
    {
        var value = GameMode.FromName("  RankedDuel  ");

        value.ShouldBe(GameMode.RankedDuel);
    }

    [Fact(DisplayName = "Game mode should reject unknown names when name is unknown")]
    public void FromName_Should_Throw_When_NameIsUnknown()
    {
        Action action = () => GameMode.FromName("Unknown");

        action.ShouldThrow<InvalidOperationException>();
    }

    [Theory(DisplayName = "Game mode lookup should report matches when id is provided")]
    [InlineData(1, true)]
    [InlineData(0, false)]
    public void TryFromId_Should_ReportMatch_When_IdIsProvided(int id, bool expected)
    {
        var found = GameMode.TryFromId(id, out var value);

        found.ShouldBe(expected);
        (value is not null).ShouldBe(expected);
    }

    [Theory(DisplayName = "Game mode lookup should report matches when name is provided")]
    [InlineData("RankedDuel", true)]
    [InlineData("Unknown", false)]
    [InlineData("", false)]
    public void TryFromName_Should_ReportMatch_When_NameIsProvided(string name, bool expected)
    {
        var found = GameMode.TryFromName(name, out var value);

        found.ShouldBe(expected);
        (value is not null).ShouldBe(expected);
    }

    [Fact(DisplayName = "Game mode should normalize known names when name is known")]
    public void Create_Should_ReturnValue_When_NameIsKnown()
    {
        var result = GameMode.Create("  RankedDuel  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(GameMode.RankedDuel);
    }

    [Theory(DisplayName = "Game mode should reject missing names when name is empty")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_Should_ReturnValidationFailure_When_NameIsEmpty(string? name)
    {
        var result = GameMode.Create(name!);

        result.ShouldHaveSingleError(ErrorType.Validation, "Game mode is required.")
            .ShouldHaveField(nameof(GameMode));
    }

    [Fact(DisplayName = "Game mode should reject unknown names when name is unknown")]
    public void Create_Should_ReturnValidationFailure_When_NameIsUnknown()
    {
        var result = GameMode.Create("Unknown");

        result.ShouldHaveSingleError(ErrorType.Validation, "Game mode 'Unknown' is invalid.")
            .ShouldHaveField(nameof(GameMode));
    }

    [Theory(DisplayName = "Game mode should provide Russian display names when name is known")]
    [InlineData("RankedDuel", "Рейтинговый 1×1")]
    [InlineData("CasualDuel", "Обычный 1×1")]
    public void Create_Should_ProvideRussianDisplayNames_When_NameIsKnown(string name, string displayName)
    {
        var value = GameMode.Create(name).Value;

        value.DisplayName.ShouldBe(displayName);
        value.Name.Length.ShouldBeLessThanOrEqualTo(GameMode.MaxLength);
    }
}
