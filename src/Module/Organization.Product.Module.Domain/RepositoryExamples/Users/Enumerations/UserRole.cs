namespace Organization.Product.Module.Domain.Users.Enumerations;

public sealed class UserRole : Enumeration<UserRole>
{
    public static readonly UserRole User = new(1, nameof(User));
    public static readonly UserRole Admin = new(2, nameof(Admin));
    public static readonly UserRole Moderator = new(3, nameof(Moderator));

    private UserRole(int id, string name)
        : base(id, name)
    {
    }

    public static Result<UserRole> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<UserRole>(
                Error.Validation("User role cannot be empty.")
                .WithField(nameof(UserRole)));
        }

        var normalizedValue = value.Trim();

        if (TryFromName(normalizedValue, out var role) && role is not null)
        {
            return Result.Success(role);
        }

        return Result.Failure<UserRole>(
            Error.Validation($"User role '{normalizedValue}' is invalid.")
            .WithField(nameof(UserRole)));
    }
}
