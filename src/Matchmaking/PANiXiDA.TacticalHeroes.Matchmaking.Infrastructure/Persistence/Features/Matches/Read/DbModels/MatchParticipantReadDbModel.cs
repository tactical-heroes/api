using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchmakingTickets.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.Matches.Read.DbModels;

public sealed class MatchParticipantReadDbModel : ReadDbModel<Guid>
{
    public Guid MatchId { get; set; }
    public Guid TicketId { get; set; }
    public Guid PlayerId { get; set; }
    public Guid BuildId { get; set; }
    public int Rating { get; set; }

    public MatchReadDbModel? Match { get; set; }
    public MatchmakingTicketReadDbModel? Ticket { get; set; }
}
