namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;

public sealed class UserStatus : Enumeration<UserStatus>
{
    public static readonly UserStatus Active = new(1, nameof(Active), "Активный");
    public static readonly UserStatus Blocked = new(2, nameof(Blocked), "Заблокирован");

    private UserStatus(int id, string name, string displayName)
        : base(id, name)
    {
        DisplayName = displayName;
    }

    public const int MaxNameLength = 50;

    public string DisplayName { get; }

    public bool IsBlocked => this == Blocked;

    public static Result<UserStatus> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<UserStatus>(
                Error.Validation("Account status is required.")
                    .WithField(nameof(UserStatus)));
        }

        var normalizedValue = value.Trim();

        return TryFromName(normalizedValue, out var status) && status is not null
            ? Result.Success(status)
            : Result.Failure<UserStatus>(
                Error.Validation($"Account status '{normalizedValue}' is invalid.")
                    .WithField(nameof(UserStatus)));
    }
}
