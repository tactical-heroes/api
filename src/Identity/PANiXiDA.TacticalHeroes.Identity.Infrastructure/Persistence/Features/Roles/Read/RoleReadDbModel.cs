namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read;

public sealed class RoleReadDbModel : AuditableReadDbModel<Guid>
{
    public string Name { get; set; } = string.Empty;
}
