using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.MatchSearches.Enumerations;

public sealed class MatchSearchStatusTests
{
    [Theory(DisplayName = "Search status should accept lifecycle states when parsed")]
    [InlineData("Searching", "Поиск матча")]
    [InlineData("Matched", "Матч найден")]
    [InlineData("Cancelled", "Поиск отменён")]
    [InlineData("Expired", "Время поиска истекло")]
    public void Create_Should_AcceptLifecycleStates_When_Parsed(string value, string displayName)
    {
        var result = MatchSearchStatus.Create(value);

        result.Value.Name.ShouldBe(value);
        result.Value.DisplayName.ShouldBe(displayName);
    }

    [Theory(DisplayName = "Search status should reject unknown values when parsed")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Unknown")]
    public void Create_Should_RejectUnknownValues_When_Parsed(string value)
    {
        var result = MatchSearchStatus.Create(value);

        result.IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Search status should display name when converted to text")]
    public void ToString_Should_DisplayName_When_ConvertedToText()
    {
        var text = MatchSearchStatus.Searching.ToString();

        text.ShouldBe("Searching");
    }

    [Fact(DisplayName = "Search status should list states when called")]
    public void GetAll_Should_ListStates_When_Called()
    {
        var values = MatchSearchStatus.GetAll();

        string[] expected =
        [
            "Searching",
            "Matched",
            "Cancelled",
            "Expired",
        ];

        values.Select(value => value.Name).ShouldBe(expected);
    }

    [Fact(DisplayName = "Search status should resolve identifier when id is known")]
    public void FromId_Should_ResolveIdentifier_When_IdIsKnown()
    {
        var value = MatchSearchStatus.FromId(1);

        value.ShouldBe(MatchSearchStatus.Searching);
    }

    [Fact(DisplayName = "Search status should resolve trimmed name when name is known")]
    public void FromName_Should_ResolveTrimmedName_When_NameIsKnown()
    {
        var value = MatchSearchStatus.FromName("  Searching  ");

        value.ShouldBe(MatchSearchStatus.Searching);
    }

    [Theory(DisplayName = "Search status should report lookup success when id is supplied")]
    [InlineData(1, true)]
    [InlineData(0, false)]
    public void TryFromId_Should_ReportLookupSuccess_When_IdIsSupplied(int id, bool expected)
    {
        var found = MatchSearchStatus.TryFromId(id, out var value);

        found.ShouldBe(expected);
        (value is not null).ShouldBe(expected);
    }

    [Theory(DisplayName = "Search status should report lookup success when name is supplied")]
    [InlineData("Searching", true)]
    [InlineData("Unknown", false)]
    public void TryFromName_Should_ReportLookupSuccess_When_NameIsSupplied(string name, bool expected)
    {
        var found = MatchSearchStatus.TryFromName(name, out var value);

        found.ShouldBe(expected);
        (value is not null).ShouldBe(expected);
    }
}
