namespace PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.ValueObjects;

public sealed class PasswordHash : ValueObject
{
    public const int MaxLength = 1024;

    private PasswordHash(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<PasswordHash> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<PasswordHash>(
                Error.Validation("Password hash cannot be empty.")
                    .WithField(nameof(PasswordHash)));
        }

        if (value.Length > MaxLength)
        {
            return Result.Failure<PasswordHash>(
                Error.Validation($"Password hash cannot be longer than {MaxLength} characters.")
                    .WithField(nameof(PasswordHash)));
        }

        return Result.Success(new PasswordHash(value));
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
