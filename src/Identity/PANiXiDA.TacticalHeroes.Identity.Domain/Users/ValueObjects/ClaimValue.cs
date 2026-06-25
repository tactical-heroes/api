namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

public sealed class ClaimValue : ValueObject
{
    public const int MaxLength = 1024;

    private ClaimValue(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<ClaimValue> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<ClaimValue>(
                Error.Validation("Claim value cannot be empty.")
                    .WithField(nameof(ClaimValue)));
        }

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > MaxLength)
        {
            return Result.Failure<ClaimValue>(
                Error.Validation($"Claim value cannot be longer than {MaxLength} characters.")
                    .WithField(nameof(ClaimValue)));
        }

        return Result.Success(new ClaimValue(normalizedValue));
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
