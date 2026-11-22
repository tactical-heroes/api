using PANiXiDA.TacticalHeroes.Games.Domain.Builds;
using PANiXiDA.TacticalHeroes.Games.Domain.Games;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Entities.GameParticipants;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Entities.GameParticipants.ValueObjects;
using PANiXiDA.TacticalHeroes.Games.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Games.UnitTests.Domain.Games.Entities.GameParticipants;

public sealed class GameParticipantTests
{
    [Fact(DisplayName = "Game participant should preserve references and rating when created")]
    public void Create_Should_PreserveReferencesAndRating_When_Created()
    {
        var gameId = GameId.New();
        var playerId = PlayerId.New();
        var buildId = BuildId.New();
        var rating = Rating.Create(1250);

        var participant = GameParticipant.Create(gameId, playerId, buildId, rating);

        participant.Id.Value.Version.ShouldBe(7);
        participant.GameId.ShouldBe(gameId);
        participant.PlayerId.ShouldBe(playerId);
        participant.BuildId.ShouldBe(buildId);
        participant.Rating.ShouldBe(rating);
    }
}
