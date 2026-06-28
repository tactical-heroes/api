using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read;

public sealed class RoleReadDbModel : AuditableReadDbModel<Guid>
{
    public string Name { get; set; } = string.Empty;

    public ICollection<UserRoleReadDbModel> Users { get; set; } = [];
    public ICollection<RoleClaimReadDbModel> Claims { get; set; } = [];
}
