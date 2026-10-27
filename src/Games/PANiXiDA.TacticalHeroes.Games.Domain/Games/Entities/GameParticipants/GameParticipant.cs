using PANiXiDA.TacticalHeroes.Games.Domain.Builds;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Entities.GameParticipants.Enumerations;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Entities.GameParticipants.ValueObjects;
using PANiXiDA.TacticalHeroes.Games.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Games.Domain.Games.Entities.GameParticipants;

public sealed class GameParticipant : Entity<GameParticipantId>
{
    private GameParticipant(
        GameParticipantId id,
        GameId gameId,
        PlayerId playerId,
        BuildId buildId,
        Rating rating,
        GameSide side) : base(id)
    {
        GameId = gameId;
        PlayerId = playerId;
        BuildId = buildId;
        Rating = rating;
        Side = side;
    }

    public GameId GameId { get; private set; }
    public PlayerId PlayerId { get; private set; }
    public BuildId BuildId { get; private set; }
    public Rating Rating { get; private set; }
    public GameSide Side { get; private set; }

    internal static GameParticipant Create(
        GameId gameId,
        PlayerId playerId,
        BuildId buildId,
        Rating rating,
        GameSide side)
    {
        return new GameParticipant(
            id: GameParticipantId.New(),
            gameId: gameId,
            playerId: playerId,
            buildId: buildId,
            rating: rating,
            side: side);
    }
}
