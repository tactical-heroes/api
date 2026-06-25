using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;

namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities;

public sealed class UserRole : Entity<RoleId>
{
    private UserRole(RoleId id)
        : base(id)
    {
    }

    public RoleId RoleId => Id;

    internal static Result<UserRole> Create(RoleId roleId)
    {
        return Result.Success(new UserRole(roleId));
    }
}
