namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read;

public sealed class RoleClaimReadDbModel : ReadDbModel<Guid>
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
}
