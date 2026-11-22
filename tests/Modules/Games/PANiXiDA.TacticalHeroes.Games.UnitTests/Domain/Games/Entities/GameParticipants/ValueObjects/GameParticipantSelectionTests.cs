using PANiXiDA.TacticalHeroes.Games.Domain.Builds;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Entities.GameParticipants.ValueObjects;
using PANiXiDA.TacticalHeroes.Games.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Games.UnitTests.Domain.Games.Entities.GameParticipants.ValueObjects;

public sealed class GameParticipantSelectionTests
{
    [Fact(DisplayName = "Selection should preserve participant data when values are provided")]
    public void Create_Should_PreserveParticipantData_When_ValuesAreProvided()
    {
        var playerId = PlayerId.New();
        var buildId = BuildId.New();
        var rating = Rating.Create(1500);

        var selection = GameParticipantSelection.Create(playerId, buildId, rating);

        selection.PlayerId.ShouldBe(playerId);
        selection.BuildId.ShouldBe(buildId);
        selection.Rating.ShouldBe(rating);
    }

    [Fact(DisplayName = "Selection should compare by value when selections are compared")]
    public void Equals_Should_CompareByValue_When_SelectionsAreCompared()
    {
        var playerId = PlayerId.New();
        var buildId = BuildId.New();
        var selection = GameParticipantSelection.Create(playerId, buildId, Rating.Create(1500));
        var same = GameParticipantSelection.Create(playerId, buildId, Rating.Create(1500));
        var otherPlayer = GameParticipantSelection.Create(PlayerId.New(), buildId, Rating.Create(1500));
        var otherBuild = GameParticipantSelection.Create(playerId, BuildId.New(), Rating.Create(1500));
        var otherRating = GameParticipantSelection.Create(playerId, buildId, Rating.Create(1600));

        selection.Equals(same).ShouldBeTrue();
        selection.GetHashCode().ShouldBe(same.GetHashCode());
        selection.Equals(otherPlayer).ShouldBeFalse();
        selection.Equals(otherBuild).ShouldBeFalse();
        selection.Equals(otherRating).ShouldBeFalse();
    }

    [Fact(DisplayName = "Selection should include participant details when converted to string")]
    public void ToString_Should_IncludeParticipantDetails_When_ConvertedToString()
    {
        var playerId = PlayerId.New();
        var buildId = BuildId.New();
        var selection = GameParticipantSelection.Create(playerId, buildId, Rating.Create(1500));

        var result = selection.ToString();

        result.ShouldContain(nameof(GameParticipantSelection));
        result.ShouldContain(playerId.ToString());
        result.ShouldContain(buildId.ToString());
        result.ShouldContain("1500");
    }
}
