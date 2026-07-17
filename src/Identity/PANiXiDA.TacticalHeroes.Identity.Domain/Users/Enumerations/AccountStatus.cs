namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;

public sealed class AccountStatus : Enumeration<AccountStatus>
{
    public static readonly AccountStatus Active = new(1, nameof(Active), "Активный");
    public static readonly AccountStatus Blocked = new(2, nameof(Blocked), "Заблокирован");

    private AccountStatus(int id, string name, string displayName)
        : base(id, name)
    {
        DisplayName = displayName;
    }

    public const int MaxNameLength = 50;

    public string DisplayName { get; }

    public bool IsBlocked => this == Blocked;

    public static Result<AccountStatus> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<AccountStatus>(
                Error.Validation("Account status is required.")
                    .WithField(nameof(AccountStatus)));
        }

        var normalizedValue = value.Trim();

        return TryFromName(normalizedValue, out var status) && status is not null
            ? Result.Success(status)
            : Result.Failure<AccountStatus>(
                Error.Validation($"Account status '{normalizedValue}' is invalid.")
                    .WithField(nameof(AccountStatus)));
    }
}
