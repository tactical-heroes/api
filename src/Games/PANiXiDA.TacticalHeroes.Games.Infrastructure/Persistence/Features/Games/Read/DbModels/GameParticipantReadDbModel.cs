namespace PANiXiDA.TacticalHeroes.Games.Infrastructure.Persistence.Features.Games.Read.DbModels;

public sealed class GameParticipantReadDbModel : ReadDbModel<Guid>
{
    public Guid GameId { get; set; }
    public Guid PlayerId { get; set; }
    public Guid BuildId { get; set; }
    public int Rating { get; set; }
    public GameReadDbModel? Game { get; set; }
}
