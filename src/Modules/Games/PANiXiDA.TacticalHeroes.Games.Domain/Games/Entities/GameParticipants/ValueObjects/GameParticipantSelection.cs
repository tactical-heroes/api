using PANiXiDA.TacticalHeroes.Games.Domain.Builds;
using PANiXiDA.TacticalHeroes.Games.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Games.Domain.Games.Entities.GameParticipants.ValueObjects;

public sealed partial class GameParticipantSelection : ValueObject
{
    private GameParticipantSelection(PlayerId playerId, BuildId buildId, Rating rating)
    {
        PlayerId = playerId;
        BuildId = buildId;
        Rating = rating;
    }

    public PlayerId PlayerId { get; }
    public BuildId BuildId { get; }
    public Rating Rating { get; }

    public static GameParticipantSelection Create(PlayerId playerId, BuildId buildId, Rating rating)
    {
        return new GameParticipantSelection(playerId: playerId, buildId: buildId, rating: rating);
    }
}
