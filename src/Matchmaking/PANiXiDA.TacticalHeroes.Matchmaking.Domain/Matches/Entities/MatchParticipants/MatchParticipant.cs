using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Builds;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.ValueObjects;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches.Entities.MatchParticipants;

public sealed class MatchParticipant : Entity<MatchParticipantId>
{
    private MatchParticipant(
        MatchParticipantId id,
        MatchId matchId,
        MatchmakingTicketId ticketId,
        PlayerId playerId,
        BuildId buildId,
        RatingSnapshot rating)
        : base(id)
    {
        MatchId = matchId;
        TicketId = ticketId;
        PlayerId = playerId;
        BuildId = buildId;
        Rating = rating;
    }

    public MatchId MatchId { get; private set; }
    public MatchmakingTicketId TicketId { get; private set; }
    public PlayerId PlayerId { get; private set; }
    public BuildId BuildId { get; private set; }
    public RatingSnapshot Rating { get; private set; }

    internal static MatchParticipant Create(MatchId matchId, MatchmakingTicket ticket)
    {
        return new MatchParticipant(
            id: MatchParticipantId.New(),
            matchId: matchId,
            ticketId: ticket.Id,
            playerId: ticket.PlayerId,
            buildId: ticket.BuildId,
            rating: ticket.Rating);
    }
}
