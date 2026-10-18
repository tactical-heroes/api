namespace PANiXiDA.TacticalHeroes.Identity.Domain.Roles.ValueObjects;

public sealed partial class RoleName : ValueObject
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
                error: Error.Validation(message: "Role name is required.")
                    .WithField(nameof(RoleName)));
        }

        var normalizedValue = value.Trim();

        return normalizedValue.Length <= MaxLength
            ? Result.Success(value: new RoleName(value: normalizedValue))
            : Result.Failure<RoleName>(
                error: Error.Validation(message: $"Role name cannot be longer than {MaxLength} characters.")
                    .WithField(nameof(RoleName)));
    }
}
