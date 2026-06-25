using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityRoles.Entities;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityRoles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Domain.IdentityRoles;

public sealed class IdentityRole : AggregateRoot<IdentityRoleId>
{
    private readonly List<IdentityRolePermission> _permissions = [];

    private IdentityRole(
        IdentityRoleId id,
        RoleName name)
        : base(id)
    {
        Name = name;
    }

    public RoleName Name { get; private set; }
    public IReadOnlyCollection<IdentityRolePermission> Permissions => _permissions;

    public static Result<IdentityRole> Create(string name)
    {
        var nameResult = RoleName.Create(name);

        if (nameResult.IsFailure)
        {
            return Result.Failure<IdentityRole>(nameResult.Errors);
        }

        return Result.Success(new IdentityRole(IdentityRoleId.New(), nameResult.Value));
    }

    public Result GrantPermission(string permissionName)
    {
        var permissionResult = IdentityRolePermission.Create(permissionName);

        if (permissionResult.IsFailure)
        {
            return Result.Failure(permissionResult.Errors);
        }

        if (_permissions.Any(permission => permission.Name == permissionResult.Value.Name))
        {
            return Result.Success();
        }

        _permissions.Add(permissionResult.Value);

        return Result.Success();
    }

    public Result RevokePermission(string permissionName)
    {
        var permissionNameResult = PermissionName.Create(permissionName);

        if (permissionNameResult.IsFailure)
        {
            return Result.Failure(permissionNameResult.Errors);
        }

        _permissions.RemoveAll(permission => permission.Name == permissionNameResult.Value);

        return Result.Success();
    }
}
