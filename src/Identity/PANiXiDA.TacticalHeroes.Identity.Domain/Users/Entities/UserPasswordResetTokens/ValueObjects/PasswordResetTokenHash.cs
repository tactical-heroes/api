namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserPasswordResetTokens.ValueObjects;

public sealed class PasswordResetTokenHash : ValueObject
{
    public const int MaxLength = 128;

    private PasswordResetTokenHash(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<PasswordResetTokenHash> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<PasswordResetTokenHash>(
                Error.Validation("Password reset token hash cannot be empty.")
                    .WithField(nameof(PasswordResetTokenHash)));
        }

        if (value.Length > MaxLength)
        {
            return Result.Failure<PasswordResetTokenHash>(
                Error.Validation($"Password reset token hash cannot be longer than {MaxLength} characters.")
                    .WithField(nameof(PasswordResetTokenHash)));
        }

        return Result.Success(new PasswordResetTokenHash(value));
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
