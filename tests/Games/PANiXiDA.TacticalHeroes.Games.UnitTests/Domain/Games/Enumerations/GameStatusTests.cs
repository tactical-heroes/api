using PANiXiDA.TacticalHeroes.Games.Domain.Games.Enumerations;

namespace PANiXiDA.TacticalHeroes.Games.UnitTests.Domain.Games.Enumerations;

public sealed class GameStatusTests
{
    [Fact(DisplayName = "Game status should preserve identifier order when called")]
    public void GetAll_Should_ReturnValuesInIdOrder_When_Called()
    {
        var values = GameStatus.GetAll();

        values.ShouldBe([
            GameStatus.WaitingForPlayers,
            GameStatus.InProgress,
            GameStatus.Finished,
            GameStatus.Cancelled]);
    }

    [Theory(DisplayName = "Game status should resolve known identifiers when id is known")]
    [InlineData(1, "WaitingForPlayers")]
    [InlineData(2, "InProgress")]
    [InlineData(3, "Finished")]
    [InlineData(4, "Cancelled")]
    public void FromId_Should_ReturnValue_When_IdIsKnown(int id, string name)
    {
        var value = GameStatus.FromId(id);

        value.Name.ShouldBe(name);
    }

    [Fact(DisplayName = "Game status should reject unknown identifiers when id is unknown")]
    public void FromId_Should_Throw_When_IdIsUnknown()
    {
        Action action = () => GameStatus.FromId(0);

        action.ShouldThrow<InvalidOperationException>();
    }

    [Fact(DisplayName = "Game status should resolve trimmed names when name is known")]
    public void FromName_Should_ReturnValue_When_NameIsKnown()
    {
        var value = GameStatus.FromName("  WaitingForPlayers  ");

        value.ShouldBe(GameStatus.WaitingForPlayers);
    }

    [Fact(DisplayName = "Game status should reject unknown names when name is unknown")]
    public void FromName_Should_Throw_When_NameIsUnknown()
    {
        Action action = () => GameStatus.FromName("Unknown");

        action.ShouldThrow<InvalidOperationException>();
    }

    [Theory(DisplayName = "Game status lookup should report matches when id is provided")]
    [InlineData(1, true)]
    [InlineData(0, false)]
    public void TryFromId_Should_ReportMatch_When_IdIsProvided(int id, bool expected)
    {
        var found = GameStatus.TryFromId(id, out var value);

        found.ShouldBe(expected);
        (value is not null).ShouldBe(expected);
    }

    [Theory(DisplayName = "Game status lookup should report matches when name is provided")]
    [InlineData("WaitingForPlayers", true)]
    [InlineData("Unknown", false)]
    [InlineData("", false)]
    public void TryFromName_Should_ReportMatch_When_NameIsProvided(string name, bool expected)
    {
        var found = GameStatus.TryFromName(name, out var value);

        found.ShouldBe(expected);
        (value is not null).ShouldBe(expected);
    }

    [Fact(DisplayName = "Game status should normalize known names when name is known")]
    public void Create_Should_ReturnValue_When_NameIsKnown()
    {
        var result = GameStatus.Create("  WaitingForPlayers  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(GameStatus.WaitingForPlayers);
    }

    [Theory(DisplayName = "Game status should reject missing names when name is empty")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_Should_ReturnValidationFailure_When_NameIsEmpty(string? name)
    {
        var result = GameStatus.Create(name!);

        result.ShouldHaveSingleError(ErrorType.Validation, "Game status is required.")
            .ShouldHaveField(nameof(GameStatus));
    }

    [Fact(DisplayName = "Game status should reject unknown names when name is unknown")]
    public void Create_Should_ReturnValidationFailure_When_NameIsUnknown()
    {
        var result = GameStatus.Create("Unknown");

        result.ShouldHaveSingleError(ErrorType.Validation, "Game status 'Unknown' is invalid.")
            .ShouldHaveField(nameof(GameStatus));
    }

    [Theory(DisplayName = "Game status should provide Russian display names when name is known")]
    [InlineData("WaitingForPlayers", "Ожидание игроков")]
    [InlineData("InProgress", "Игра идёт")]
    [InlineData("Finished", "Игра завершена")]
    [InlineData("Cancelled", "Игра отменена")]
    public void Create_Should_ProvideRussianDisplayNames_When_NameIsKnown(string name, string displayName)
    {
        var value = GameStatus.Create(name).Value;

        value.DisplayName.ShouldBe(displayName);
        value.Name.Length.ShouldBeLessThanOrEqualTo(GameStatus.MaxLength);
    }
}
