using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;

namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities;

public sealed class UserRole : Entity<Guid>
{
    private UserRole(
        Guid id,
        RoleId roleId)
        : base(id)
    {
        RoleId = roleId;
    }

    public RoleId RoleId { get; private set; }

    internal static Result<UserRole> Create(RoleId roleId)
    {
        return Result.Success(new UserRole(Guid.CreateVersion7(), roleId));
    }
}
