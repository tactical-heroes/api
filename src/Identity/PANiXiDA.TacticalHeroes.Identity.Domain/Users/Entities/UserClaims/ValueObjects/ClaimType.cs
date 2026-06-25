namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims.ValueObjects;

public sealed class ClaimType : ValueObject
{
    public const int MaxLength = 256;

    private ClaimType(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<ClaimType> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<ClaimType>(
                Error.Validation("Claim type cannot be empty.")
                    .WithField(nameof(ClaimType)));
        }

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > MaxLength)
        {
            return Result.Failure<ClaimType>(
                Error.Validation($"Claim type cannot be longer than {MaxLength} characters.")
                    .WithField(nameof(ClaimType)));
        }

        return Result.Success(new ClaimType(normalizedValue));
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
