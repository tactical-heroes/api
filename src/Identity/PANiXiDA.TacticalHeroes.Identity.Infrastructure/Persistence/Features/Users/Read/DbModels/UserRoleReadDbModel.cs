using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

public sealed class UserRoleReadDbModel
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    public UserReadDbModel? User { get; set; }
    public RoleReadDbModel? Role { get; set; }
}
