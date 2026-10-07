namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.DbModels;

public sealed class RoleClaimReadDbModel : ReadDbModel<int>
{
    public Guid RoleId { get; set; }
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;

    public RoleReadDbModel? Role { get; set; }
}
