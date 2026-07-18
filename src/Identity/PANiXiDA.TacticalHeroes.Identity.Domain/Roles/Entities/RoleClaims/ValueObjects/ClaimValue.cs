namespace PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims.ValueObjects;

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
                error: Error.Validation(message: "Claim value cannot be empty.")
                    .WithField(nameof(ClaimValue)));
        }

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > MaxLength)
        {
            return Result.Failure<ClaimValue>(
                error: Error.Validation(message: $"Claim value cannot be longer than {MaxLength} characters.")
                    .WithField(nameof(ClaimValue)));
        }

        return Result.Success(value: new ClaimValue(value: normalizedValue));
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
