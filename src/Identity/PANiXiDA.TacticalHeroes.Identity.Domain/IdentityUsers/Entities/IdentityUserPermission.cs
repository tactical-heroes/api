using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityRoles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.Entities;

public sealed class IdentityUserPermission : Entity<Guid>
{
    private IdentityUserPermission(
        Guid id,
        PermissionName name)
        : base(id)
    {
        Name = name;
    }

    public PermissionName Name { get; private set; }

    internal static Result<IdentityUserPermission> Create(string permissionName)
    {
        var permissionNameResult = PermissionName.Create(permissionName);

        if (permissionNameResult.IsFailure)
        {
            return Result.Failure<IdentityUserPermission>(permissionNameResult.Errors);
        }

        return Result.Success(new IdentityUserPermission(Guid.CreateVersion7(), permissionNameResult.Value));
    }
}
