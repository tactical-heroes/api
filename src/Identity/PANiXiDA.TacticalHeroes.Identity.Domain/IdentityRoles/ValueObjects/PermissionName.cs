namespace PANiXiDA.TacticalHeroes.Identity.Domain.IdentityRoles.ValueObjects;

public sealed class PermissionName : ValueObject
{
    public const int MaxLength = 256;

    private PermissionName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<PermissionName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<PermissionName>(
                Error.Validation("Permission name cannot be empty.")
                    .WithField(nameof(PermissionName)));
        }

        var normalizedValue = value.Trim().ToLowerInvariant();

        if (normalizedValue.Length > MaxLength)
        {
            return Result.Failure<PermissionName>(
                Error.Validation($"Permission name cannot be longer than {MaxLength} characters.")
                    .WithField(nameof(PermissionName)));
        }

        return Result.Success(new PermissionName(normalizedValue));
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
