using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Builds;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches.Entities.MatchParticipants;

public sealed class MatchParticipant : Entity<MatchParticipantId>
{
    private MatchParticipant(
        MatchParticipantId id,
        MatchId matchId,
        MatchSearchId matchSearchId,
        PlayerId playerId,
        BuildId buildId,
        RatingSnapshot rating)
        : base(id)
    {
        MatchId = matchId;
        MatchSearchId = matchSearchId;
        PlayerId = playerId;
        BuildId = buildId;
        Rating = rating;
    }

    public MatchId MatchId { get; private set; }
    public MatchSearchId MatchSearchId { get; private set; }
    public PlayerId PlayerId { get; private set; }
    public BuildId BuildId { get; private set; }
    public RatingSnapshot Rating { get; private set; }

    internal static MatchParticipant Create(MatchId matchId, MatchSearch search)
    {
        return new MatchParticipant(
            id: MatchParticipantId.New(),
            matchId: matchId,
            matchSearchId: search.Id,
            playerId: search.PlayerId,
            buildId: search.BuildId,
            rating: search.Rating);
    }
}
