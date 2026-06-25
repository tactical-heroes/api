using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityRoles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Domain.IdentityRoles.Entities;

public sealed class IdentityRolePermission : Entity<Guid>
{
    private IdentityRolePermission(
        Guid id,
        PermissionName name)
        : base(id)
    {
        Name = name;
    }

    public PermissionName Name { get; private set; }

    internal static Result<IdentityRolePermission> Create(string permissionName)
    {
        var permissionNameResult = PermissionName.Create(permissionName);

        if (permissionNameResult.IsFailure)
        {
            return Result.Failure<IdentityRolePermission>(permissionNameResult.Errors);
        }

        return Result.Success(new IdentityRolePermission(Guid.CreateVersion7(), permissionNameResult.Value));
    }
}
