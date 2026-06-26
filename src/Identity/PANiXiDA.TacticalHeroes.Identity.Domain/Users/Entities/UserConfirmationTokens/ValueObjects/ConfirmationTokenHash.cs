namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserConfirmationTokens.ValueObjects;

public sealed class ConfirmationTokenHash : ValueObject
{
    public const int MaxLength = 128;

    private ConfirmationTokenHash(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<ConfirmationTokenHash> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<ConfirmationTokenHash>(
                Error.Validation("Confirmation token hash cannot be empty.")
                    .WithField(nameof(ConfirmationTokenHash)));
        }

        if (value.Length > MaxLength)
        {
            return Result.Failure<ConfirmationTokenHash>(
                Error.Validation($"Confirmation token hash cannot be longer than {MaxLength} characters.")
                    .WithField(nameof(ConfirmationTokenHash)));
        }

        return Result.Success(new ConfirmationTokenHash(value));
    }

    public override string ToString()
    {
        return Value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
