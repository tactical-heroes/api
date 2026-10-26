using PANiXiDA.TacticalHeroes.Matchmaking.Domain.BuildSnapshots;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.BuildSnapshots;

public sealed class BuildSnapshotIdTests
{
    [Fact(DisplayName = "Build snapshot id should reject empty value when created")]
    public void Create_Should_RejectEmptyValue_When_Created()
    {
        var value = Guid.CreateVersion7();
        var valid = BuildSnapshotId.Create(value);
        var empty = BuildSnapshotId.Create(Guid.Empty);

        valid.Value.Value.ShouldBe(value);
        empty.IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Build snapshot id should generate distinct values when requested")]
    public void New_Should_GenerateDistinctValues_When_Requested()
    {
        var first = BuildSnapshotId.New();

        first.Value.Version.ShouldBe(7);
        first.ShouldNotBe(BuildSnapshotId.New());
    }

    [Fact(DisplayName = "Build snapshot id should display underlying value when converted to text")]
    public void ToString_Should_DisplayUnderlyingValue_When_ConvertedToText()
    {
        var id = BuildSnapshotId.New();

        id.ToString().ShouldBe(id.Value.ToString());
    }
}
