using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Builds;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.Builds;

public sealed class BuildIdTests
{
    [Fact(DisplayName = "Build id should reject empty value when created")]
    public void Create_Should_RejectEmptyValue_When_Created()
    {
        var value = Guid.CreateVersion7();
        var valid = BuildId.Create(value);
        var empty = BuildId.Create(Guid.Empty);

        valid.Value.Value.ShouldBe(value);
        empty.IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Build id should generate distinct values when requested")]
    public void New_Should_GenerateDistinctValues_When_Requested()
    {
        var first = BuildId.New();

        first.Value.Version.ShouldBe(7);
        first.ShouldNotBe(BuildId.New());
    }

    [Fact(DisplayName = "Build id should display underlying value when converted to text")]
    public void ToString_Should_DisplayUnderlyingValue_When_ConvertedToText()
    {
        var id = BuildId.New();

        id.ToString().ShouldBe(id.Value.ToString());
    }
}
