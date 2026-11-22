using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.MatchSearches.Enumerations;

public sealed class MatchmakingModeTests
{
    [Theory(DisplayName = "Mode should accept supported modes when parsed")]
    [InlineData("RankedDuel", "Рейтинговый 1×1")]
    [InlineData("CasualDuel", "Обычный 1×1")]
    public void Create_Should_AcceptSupportedModes_When_Parsed(string value, string displayName)
    {
        var result = MatchmakingMode.Create(value);

        result.Value.Name.ShouldBe(value);
        result.Value.DisplayName.ShouldBe(displayName);
    }

    [Theory(DisplayName = "Mode should reject unknown values when parsed")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Unknown")]
    [InlineData("duel")]
    [InlineData("other")]
    public void Create_Should_RejectUnknownValues_When_Parsed(string value)
    {
        var result = MatchmakingMode.Create(value);

        result.IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Mode should return name when converted to text")]
    public void ToString_Should_ReturnName_When_ConvertedToText()
    {
        var text = MatchmakingMode.RankedDuel.ToString();

        text.ShouldBe("RankedDuel");
    }

    [Fact(DisplayName = "Mode should list modes when called")]
    public void GetAll_Should_ListModes_When_Called()
    {
        var values = MatchmakingMode.GetAll();

        string[] expected =
        [
            "RankedDuel",
            "CasualDuel",
        ];

        values.Select(value => value.Name).ShouldBe(expected);
    }

    [Fact(DisplayName = "Mode should resolve identifier when id is known")]
    public void FromId_Should_ResolveIdentifier_When_IdIsKnown()
    {
        var value = MatchmakingMode.FromId(1);

        value.ShouldBe(MatchmakingMode.RankedDuel);
    }

    [Fact(DisplayName = "Mode should resolve trimmed name when name is known")]
    public void FromName_Should_ResolveTrimmedName_When_NameIsKnown()
    {
        var value = MatchmakingMode.FromName("  RankedDuel  ");

        value.ShouldBe(MatchmakingMode.RankedDuel);
    }

    [Theory(DisplayName = "Mode should report lookup success when id is supplied")]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(0, false)]
    public void TryFromId_Should_ReportLookupSuccess_When_IdIsSupplied(int id, bool expected)
    {
        var found = MatchmakingMode.TryFromId(id, out var value);

        found.ShouldBe(expected);
        (value is not null).ShouldBe(expected);
    }

    [Theory(DisplayName = "Mode should report lookup success when name is supplied")]
    [InlineData("RankedDuel", true)]
    [InlineData("Unknown", false)]
    public void TryFromName_Should_ReportLookupSuccess_When_NameIsSupplied(string name, bool expected)
    {
        var found = MatchmakingMode.TryFromName(name, out var value);

        found.ShouldBe(expected);
        (value is not null).ShouldBe(expected);
    }
}
