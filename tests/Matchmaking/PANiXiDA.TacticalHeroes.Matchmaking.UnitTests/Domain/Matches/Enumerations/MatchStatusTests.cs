using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches.Enumerations;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.Matches.Enumerations;

public sealed class MatchStatusTests
{
    [Theory(DisplayName = "Match status should accept lifecycle states when parsed")]
    [InlineData("CreatingGame", "Создание игры")]
    [InlineData("Ready", "Игра создана")]
    [InlineData("Expired", "Время ожидания истекло")]
    [InlineData("Failed", "Ошибка создания игры")]
    public void Create_Should_AcceptLifecycleStates_When_Parsed(string value, string displayName)
    {
        var result = MatchStatus.Create(value);

        result.Value.Name.ShouldBe(value);
        result.Value.DisplayName.ShouldBe(displayName);
    }

    [Theory(DisplayName = "Match status should reject unknown values when parsed")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Unknown")]
    public void Create_Should_RejectUnknownValues_When_Parsed(string value)
    {
        var result = MatchStatus.Create(value);

        result.IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Match status should display name when converted to text")]
    public void ToString_Should_DisplayName_When_ConvertedToText()
    {
        var text = MatchStatus.CreatingGame.ToString();

        text.ShouldBe("CreatingGame");
    }

    [Fact(DisplayName = "Match status should list states when called")]
    public void GetAll_Should_ListStates_When_Called()
    {
        var values = MatchStatus.GetAll();

        string[] expected =
        [
            "CreatingGame",
            "Ready",
            "Expired",
            "Failed",
        ];

        values.Select(value => value.Name).ShouldBe(expected);
    }

    [Fact(DisplayName = "Match status should resolve identifier when id is known")]
    public void FromId_Should_ResolveIdentifier_When_IdIsKnown()
    {
        var value = MatchStatus.FromId(1);

        value.ShouldBe(MatchStatus.CreatingGame);
    }

    [Fact(DisplayName = "Match status should resolve trimmed name when name is known")]
    public void FromName_Should_ResolveTrimmedName_When_NameIsKnown()
    {
        var value = MatchStatus.FromName("  CreatingGame  ");

        value.ShouldBe(MatchStatus.CreatingGame);
    }

    [Theory(DisplayName = "Match status should report lookup success when id is supplied")]
    [InlineData(1, true)]
    [InlineData(0, false)]
    public void TryFromId_Should_ReportLookupSuccess_When_IdIsSupplied(int id, bool expected)
    {
        var found = MatchStatus.TryFromId(id, out var value);

        found.ShouldBe(expected);
        (value is not null).ShouldBe(expected);
    }

    [Theory(DisplayName = "Match status should report lookup success when name is supplied")]
    [InlineData("CreatingGame", true)]
    [InlineData("Unknown", false)]
    public void TryFromName_Should_ReportLookupSuccess_When_NameIsSupplied(string name, bool expected)
    {
        var found = MatchStatus.TryFromName(name, out var value);

        found.ShouldBe(expected);
        (value is not null).ShouldBe(expected);
    }
}
