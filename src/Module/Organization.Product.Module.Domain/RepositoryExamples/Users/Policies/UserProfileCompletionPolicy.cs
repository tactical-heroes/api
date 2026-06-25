using Organization.Product.Module.Domain.Users.Enumerations;

namespace Organization.Product.Module.Domain.Users.Policies;

public static class UserProfileCompletionPolicy
{
    public static Result EnsureCompleted(User user, DateOnly onDate)
    {
        if (user.Avatar is null && IsPrivileged(user.Role))
        {
            return Result.Failure(
                Error.Validation("Privileged users must have an avatar.")
                    .WithField(nameof(User.Avatar)));
        }

        if (user.BirthDate.GetAge(onDate) < 21 && IsPrivileged(user.Role))
        {
            return Result.Failure(
                Error.Validation("Privileged users must be at least 21 years old.")
                    .WithField(nameof(User.BirthDate)));
        }

        return Result.Success();
    }

    private static bool IsPrivileged(UserRole role)
    {
        return role == UserRole.Admin || role == UserRole.Moderator;
    }
}
