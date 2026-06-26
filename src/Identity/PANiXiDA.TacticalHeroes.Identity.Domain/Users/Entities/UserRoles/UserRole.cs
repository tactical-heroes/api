using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;

namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserRoles;

public sealed class UserRole : Entity<UserRoleId>
{
    private UserRole(
        UserId userId,
        RoleId roleId)
        : base(UserRoleId.Create(userId, roleId).Value)
    {
        UserId = userId;
        RoleId = roleId;
    }

    public UserId UserId { get; private set; }
    public RoleId RoleId { get; private set; }

    internal static Result<UserRole> Create(
        UserId userId,
        RoleId roleId)
    {
        var idResult = UserRoleId.Create(userId, roleId);

        if (idResult.IsFailure)
        {
            return Result.Failure<UserRole>(idResult.Errors);
        }

        return Result.Success(new UserRole(
            userId,
            roleId));
    }
}
