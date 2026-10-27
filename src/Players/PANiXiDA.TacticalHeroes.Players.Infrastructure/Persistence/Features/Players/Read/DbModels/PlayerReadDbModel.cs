namespace PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Features.Players.Read.DbModels;

public sealed class PlayerReadDbModel : AuditableReadDbModel<Guid>
{
    public Guid UserId { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public string NormalizedNickname { get; set; } = string.Empty;
    public Guid? AvatarFileId { get; set; }
    public int Rating { get; set; }
}
