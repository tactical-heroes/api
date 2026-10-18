using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;

namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users;

public static class UserRoleAssignments
{
    public static Result<IReadOnlyCollection<RoleId>> Create(IReadOnlyCollection<Guid> values)
    {
        if (values is null)
        {
            return Result.Failure<IReadOnlyCollection<RoleId>>(
                error: Error.Validation(message: "Role IDs are required.").WithField(nameof(User.RoleIds)));
        }

        var roleIds = new List<RoleId>();
        var seen = new HashSet<Guid>();
        foreach (var value in values)
        {
            if (!seen.Add(value))
            {
                return Result.Failure<IReadOnlyCollection<RoleId>>(
                    error: Error.Validation(message: "Role IDs must be unique.").WithField(nameof(User.RoleIds)));
            }

            var result = RoleId.Create(value);
            if (result.IsFailure)
            {
                return Result.Failure<IReadOnlyCollection<RoleId>>(errors: result.Errors);
            }

            roleIds.Add(result.Value);
        }

        return Result.Success<IReadOnlyCollection<RoleId>>(value: roleIds);
    }
}
