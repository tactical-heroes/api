namespace PANiXiDA.TacticalHeroes.Identity.Domain.Roles.ValueObjects;

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
                error: Error.Validation(message: "Role name cannot be empty.")
                    .WithField(nameof(RoleName)));
        }

        var normalizedValue = value.Trim().ToLowerInvariant();

        if (normalizedValue.Length > MaxLength)
        {
            return Result.Failure<RoleName>(
                error: Error.Validation(message: $"Role name cannot be longer than {MaxLength} characters.")
                    .WithField(nameof(RoleName)));
        }

        return Result.Success(value: new RoleName(value: normalizedValue));
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
