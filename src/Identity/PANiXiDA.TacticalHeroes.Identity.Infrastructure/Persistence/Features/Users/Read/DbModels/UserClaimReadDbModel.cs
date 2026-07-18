namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

public sealed class UserClaimReadDbModel : ReadDbModel<int>
{
    public Guid UserId { get; set; }
    public string? ClaimType { get; set; }
    public string? ClaimValue { get; set; }

    public UserReadDbModel? User { get; set; }
}
