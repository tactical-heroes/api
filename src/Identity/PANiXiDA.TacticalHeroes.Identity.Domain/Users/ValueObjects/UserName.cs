namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

public sealed class UserName : ValueObject
{
    public const int MaxLength = 256;

    private UserName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<UserName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<UserName>(
                Error.Validation("User name is required.")
                    .WithField(nameof(UserName)));
        }

        var normalizedValue = value.Trim();

        return normalizedValue.Length <= MaxLength
            ? Result.Success(new UserName(normalizedValue))
            : Result.Failure<UserName>(
                Error.Validation($"User name cannot be longer than {MaxLength} characters.")
                    .WithField(nameof(UserName)));
    }

    public override string ToString()
    {
        return Value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
