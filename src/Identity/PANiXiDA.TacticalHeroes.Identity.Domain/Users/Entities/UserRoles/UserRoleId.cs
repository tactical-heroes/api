using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;

namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserRoles;

public readonly record struct UserRoleId
{
    private UserRoleId(
        UserId userId,
        RoleId roleId)
    {
        UserId = userId;
        RoleId = roleId;
    }

    public UserId UserId { get; }
    public RoleId RoleId { get; }

    public static Result<UserRoleId> Create(
        UserId userId,
        RoleId roleId)
    {
        if (userId.Value == Guid.Empty)
        {
            return Result.Failure<UserRoleId>(
                Error.Validation("User id cannot be empty."));
        }

        if (roleId.Value == Guid.Empty)
        {
            return Result.Failure<UserRoleId>(
                Error.Validation("Role id cannot be empty."));
        }

        return Result.Success(new UserRoleId(userId, roleId));
    }

    public override string ToString()
    {
        return $"{UserId}:{RoleId}";
    }
}
