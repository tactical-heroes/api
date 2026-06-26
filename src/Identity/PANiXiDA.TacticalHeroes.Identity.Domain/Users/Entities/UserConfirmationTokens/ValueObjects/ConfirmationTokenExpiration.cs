namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserConfirmationTokens.ValueObjects;

public sealed class ConfirmationTokenExpiration : ValueObject
{
    private ConfirmationTokenExpiration(DateTimeOffset value)
    {
        Value = value;
    }

    public DateTimeOffset Value { get; }

    public static Result<ConfirmationTokenExpiration> Create(DateTimeOffset value)
    {
        if (value == default)
        {
            return Result.Failure<ConfirmationTokenExpiration>(
                Error.Validation("Confirmation token expiration cannot be empty.")
                    .WithField(nameof(ConfirmationTokenExpiration)));
        }

        return Result.Success(new ConfirmationTokenExpiration(value));
    }

    public static Result<ConfirmationTokenExpiration> CreateFuture(
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
            return Result.Failure<ConfirmationTokenExpiration>(
                Error.Validation("Confirmation token expiration must be in the future.")
                    .WithField(nameof(ConfirmationTokenExpiration)));
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
