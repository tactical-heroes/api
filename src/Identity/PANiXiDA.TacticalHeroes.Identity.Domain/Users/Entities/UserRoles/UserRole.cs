using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;

namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserRoles;

public sealed class UserRole : Entity<UserRoleId>
{
    private UserRole(
        UserId userId,
        RoleId roleId)
        : this(UserRoleId.Create(userId, roleId).Value)
    {
    }

    private UserRole(UserRoleId id)
        : base(id)
    {
        UserId = id.UserId;
        RoleId = id.RoleId;
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

        return Result.Success(new UserRole(idResult.Value));
    }
}
