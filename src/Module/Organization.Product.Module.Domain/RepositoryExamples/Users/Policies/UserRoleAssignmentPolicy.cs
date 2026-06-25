using Organization.Product.Module.Domain.Users.Enumerations;

namespace Organization.Product.Module.Domain.Users.Policies;

public static class UserRoleAssignmentPolicy
{
    public static Result EnsureCanAssign(User actor, User target, UserRole role)
    {
        if (actor.Id == target.Id)
        {
            return Result.Failure(
                Error.Validation("User cannot change own role.")
                    .WithField(nameof(User.Role)));
        }

        if (!IsAdmin(actor) && IsPrivileged(role))
        {
            return Result.Failure(
                Error.Validation("Only admins can assign privileged roles.")
                    .WithField(nameof(User.Role)));
        }

        if (IsAdmin(target) && !IsAdmin(actor))
        {
            return Result.Failure(
                Error.Validation("Only admins can change another admin role.")
                    .WithField(nameof(User.Role)));
        }

        return Result.Success();
    }

    private static bool IsAdmin(User user)
    {
        return user.Role == UserRole.Admin;
    }

    private static bool IsPrivileged(UserRole role)
    {
        return role == UserRole.Admin || role == UserRole.Moderator;
    }
}
