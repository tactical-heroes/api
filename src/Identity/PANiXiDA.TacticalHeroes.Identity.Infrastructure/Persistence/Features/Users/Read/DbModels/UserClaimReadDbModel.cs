namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

public sealed class UserClaimReadDbModel : ReadDbModel<Guid>
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public Guid UserId { get; set; }

    public UserReadDbModel? User { get; set; }
}
