namespace PANiXiDA.TacticalHeroes.Identity.Domain.IdentityRoles.ValueObjects;

public sealed class RoleName : ValueObject
{
    public const int MaxLength = 128;

    private RoleName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<RoleName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<RoleName>(
                Error.Validation("Role name cannot be empty.")
                    .WithField(nameof(RoleName)));
        }

        var normalizedValue = value.Trim().ToLowerInvariant();

        if (normalizedValue.Length > MaxLength)
        {
            return Result.Failure<RoleName>(
                Error.Validation($"Role name cannot be longer than {MaxLength} characters.")
                    .WithField(nameof(RoleName)));
        }

        return Result.Success(new RoleName(normalizedValue));
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
