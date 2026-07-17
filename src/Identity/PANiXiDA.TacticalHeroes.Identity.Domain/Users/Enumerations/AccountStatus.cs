namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;

public sealed class AccountStatus : Enumeration<AccountStatus>
{
    public static readonly AccountStatus Active = new(id: 1, name: nameof(Active), displayName: "Активный");
    public static readonly AccountStatus Blocked = new(id: 2, name: nameof(Blocked), displayName: "Заблокирован");

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
                error: Error.Validation(message: "Account status is required.")
                    .WithField(nameof(AccountStatus)));
        }

        var normalizedValue = value.Trim();

        return TryFromName(normalizedValue, out var status) && status is not null
            ? Result.Success(value: status)
            : Result.Failure<AccountStatus>(
                error: Error.Validation(message: $"Account status '{normalizedValue}' is invalid.")
                    .WithField(nameof(AccountStatus)));
    }
}
