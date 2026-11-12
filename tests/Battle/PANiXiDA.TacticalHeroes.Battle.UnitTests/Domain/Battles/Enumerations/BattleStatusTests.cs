using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Enumerations;

namespace PANiXiDA.TacticalHeroes.Battle.UnitTests.Domain.Battles.Enumerations;

public sealed class BattleStatusTests
{
    [Fact(DisplayName = "Battle status should return values in identifier order when called")]
    public void GetAll_Should_ReturnValuesInIdOrder_When_Called()
    {
        var values = BattleStatus.GetAll();

        values.ShouldBe([BattleStatus.Preparing, BattleStatus.InProgress]);
    }

    [Theory(DisplayName = "Battle status should resolve identifiers when id is known")]
    [InlineData(1, "Preparing")]
    [InlineData(2, "InProgress")]
    public void FromId_Should_ReturnValue_When_IdIsKnown(int id, string name)
    {
        var value = BattleStatus.FromId(id);

        value.Name.ShouldBe(name);
    }

    [Fact(DisplayName = "Battle status should resolve trimmed names when name is known")]
    public void FromName_Should_ReturnValue_When_NameIsKnown()
    {
        var value = BattleStatus.FromName("  Preparing  ");

        value.ShouldBe(BattleStatus.Preparing);
    }

    [Theory(DisplayName = "Battle status should report a match when id is provided")]
    [InlineData(1, true)]
    [InlineData(0, false)]
    public void TryFromId_Should_ReportMatch_When_IdIsProvided(int id, bool expected)
    {
        var found = BattleStatus.TryFromId(id, out var value);

        found.ShouldBe(expected);
        (value is not null).ShouldBe(expected);
    }

    [Theory(DisplayName = "Battle status should report a match when name is provided")]
    [InlineData("Preparing", true)]
    [InlineData("unknown", false)]
    public void TryFromName_Should_ReportMatch_When_NameIsProvided(string name, bool expected)
    {
        var found = BattleStatus.TryFromName(name, out var value);

        found.ShouldBe(expected);
        (value is not null).ShouldBe(expected);
    }

    [Theory(DisplayName = "Battle status should resolve supported names when name is valid")]
    [InlineData("Preparing")]
    [InlineData("InProgress")]
    public void Create_Should_ResolveStatus_When_NameIsValid(string name)
    {
        var result = BattleStatus.Create(name);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe(name);
    }

    [Fact(DisplayName = "Battle status should reject unsupported names when name is invalid")]
    public void Create_Should_RejectStatus_When_NameIsInvalid()
    {
        var result = BattleStatus.Create("unknown");

        result.IsFailure.ShouldBeTrue();
    }
}
