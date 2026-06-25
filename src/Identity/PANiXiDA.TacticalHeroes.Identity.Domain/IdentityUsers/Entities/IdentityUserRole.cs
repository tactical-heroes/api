using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityRoles;

namespace PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.Entities;

public sealed class IdentityUserRole : Entity<Guid>
{
    private IdentityUserRole(
        Guid id,
        IdentityRoleId roleId)
        : base(id)
    {
        RoleId = roleId;
    }

    public IdentityRoleId RoleId { get; private set; }

    internal static Result<IdentityUserRole> Create(IdentityRoleId roleId)
    {
        return Result.Success(new IdentityUserRole(Guid.CreateVersion7(), roleId));
    }
}
