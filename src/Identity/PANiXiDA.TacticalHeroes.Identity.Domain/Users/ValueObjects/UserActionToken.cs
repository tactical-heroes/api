namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

public sealed partial class UserActionToken : ValueObject
{
    private UserActionToken(
        string value,
        DateTimeOffset expiresAtUtc)
    {
        Value = value;
        ExpiresAtUtc = expiresAtUtc;
    }

    public string Value { get; }
    public DateTimeOffset ExpiresAtUtc { get; }

    public static Result<UserActionToken> Create(
        string value,
        DateTimeOffset expiresAtUtc)
    {
        return string.IsNullOrWhiteSpace(value)
            ? Result.Failure<UserActionToken>(
                error: Error.Validation(message: "User action token cannot be empty.")
                    .WithField(nameof(UserActionToken)))
            : Result.Success(value: new UserActionToken(value: value, expiresAtUtc: expiresAtUtc));
    }

    public override string ToString()
    {
        return nameof(UserActionToken);
    }
}
