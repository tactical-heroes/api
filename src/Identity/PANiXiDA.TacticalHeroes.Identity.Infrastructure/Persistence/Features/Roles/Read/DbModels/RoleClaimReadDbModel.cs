namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.DbModels;

public sealed class RoleClaimReadDbModel : ReadDbModel<int>
{
    public Guid RoleId { get; set; }
    public string? ClaimType { get; set; }
    public string? ClaimValue { get; set; }

    public RoleReadDbModel? Role { get; set; }
}
