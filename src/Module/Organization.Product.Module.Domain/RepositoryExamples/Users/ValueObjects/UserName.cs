namespace Organization.Product.Module.Domain.Users.ValueObjects;

public sealed class UserName : ValueObject
{
    public const int MaxLength = 200;

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
                Error.Validation("User name cannot be empty.")
                .WithField(nameof(UserName)));
        }

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > MaxLength)
        {
            return Result.Failure<UserName>(
                Error.Validation($"User name cannot be longer than {MaxLength} characters.")
                .WithField(nameof(UserName)));
        }

        return Result.Success(new UserName(normalizedValue));
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
