using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Enumerations;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.MatchAssignments.Enumerations;

public sealed class MatchAssignmentStatusTests
{
    [Theory(DisplayName = "Matchmaking should accept lifecycle states when parsed")]
    [InlineData("AwaitingAcceptance")]
    [InlineData("CreatingGame")]
    [InlineData("Ready")]
    [InlineData("Cancelled")]
    [InlineData("Expired")]
    [InlineData("Failed")]
    public void Create_Should_AcceptLifecycleStates_When_Parsed(string value)
    {
        var result = MatchAssignmentStatus.Create(value);

        result.Value.Name.ShouldBe(value);
    }

    [Theory(DisplayName = "Matchmaking should reject unknown values when parsed")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Unknown")]
    public void Create_Should_RejectUnknownValues_When_Parsed(string value)
    {
        var result = MatchAssignmentStatus.Create(value);

        result.IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Matchmaking should display name when converted to text")]
    public void ToString_Should_DisplayName_When_ConvertedToText()
    {
        var text = MatchAssignmentStatus.AwaitingAcceptance.ToString();

        text.ShouldBe("AwaitingAcceptance");
    }

    [Fact(DisplayName = "Matchmaking should list states when called")]
    public void GetAll_Should_ListStates_When_Called()
    {
        var values = MatchAssignmentStatus.GetAll();

        string[] expected =
        [
            "AwaitingAcceptance",
            "CreatingGame",
            "Ready",
            "Cancelled",
            "Expired",
            "Failed",
        ];

        values.Select(value => value.Name).ShouldBe(expected);
    }

    [Fact(DisplayName = "Matchmaking should resolve identifier when id is known")]
    public void FromId_Should_ResolveIdentifier_When_IdIsKnown()
    {
        var value = MatchAssignmentStatus.FromId(1);

        value.ShouldBe(MatchAssignmentStatus.AwaitingAcceptance);
    }

    [Fact(DisplayName = "Matchmaking should resolve trimmed name when name is known")]
    public void FromName_Should_ResolveTrimmedName_When_NameIsKnown()
    {
        var value = MatchAssignmentStatus.FromName("  AwaitingAcceptance  ");

        value.ShouldBe(MatchAssignmentStatus.AwaitingAcceptance);
    }

    [Theory(DisplayName = "Matchmaking should report lookup success when id is supplied")]
    [InlineData(1, true)]
    [InlineData(0, false)]
    public void TryFromId_Should_ReportLookupSuccess_When_IdIsSupplied(int id, bool expected)
    {
        var found = MatchAssignmentStatus.TryFromId(id, out var value);

        found.ShouldBe(expected);
        (value is not null).ShouldBe(expected);
    }

    [Theory(DisplayName = "Matchmaking should report lookup success when name is supplied")]
    [InlineData("AwaitingAcceptance", true)]
    [InlineData("Unknown", false)]
    public void TryFromName_Should_ReportLookupSuccess_When_NameIsSupplied(string name, bool expected)
    {
        var found = MatchAssignmentStatus.TryFromName(name, out var value);

        found.ShouldBe(expected);
        (value is not null).ShouldBe(expected);
    }
}
