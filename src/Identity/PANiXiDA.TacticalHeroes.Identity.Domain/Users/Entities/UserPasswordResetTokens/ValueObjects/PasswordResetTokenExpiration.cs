namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserPasswordResetTokens.ValueObjects;

public sealed class PasswordResetTokenExpiration : ValueObject
{
    private PasswordResetTokenExpiration(DateTimeOffset value)
    {
        Value = value;
    }

    public DateTimeOffset Value { get; }

    public static Result<PasswordResetTokenExpiration> Create(DateTimeOffset value)
    {
        if (value == default)
        {
            return Result.Failure<PasswordResetTokenExpiration>(
                Error.Validation("Password reset token expiration cannot be empty.")
                    .WithField(nameof(PasswordResetTokenExpiration)));
        }

        return Result.Success(new PasswordResetTokenExpiration(value));
    }

    public static Result<PasswordResetTokenExpiration> CreateFuture(
        DateTimeOffset value,
        DateTimeOffset nowUtc)
    {
        var expirationResult = Create(value);

        if (expirationResult.IsFailure)
        {
            return expirationResult;
        }

        if (value <= nowUtc)
        {
            return Result.Failure<PasswordResetTokenExpiration>(
                Error.Validation("Password reset token expiration must be in the future.")
                    .WithField(nameof(PasswordResetTokenExpiration)));
        }

        return expirationResult;
    }

    public bool IsExpired(DateTimeOffset nowUtc)
    {
        return Value < nowUtc;
    }

    public override string ToString()
    {
        return Value.ToString("O");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
