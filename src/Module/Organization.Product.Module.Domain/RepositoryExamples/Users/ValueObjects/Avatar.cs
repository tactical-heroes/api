namespace Organization.Product.Module.Domain.Users.ValueObjects;

public sealed class Avatar : ValueObject
{
    public const int MaxLength = 2048;

    private Avatar(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Avatar?> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Success<Avatar?>(null);
        }

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > MaxLength)
        {
            return Result.Failure<Avatar?>(
                Error.Validation($"Avatar cannot be longer than {MaxLength} characters.")
                .WithField(nameof(Avatar)));
        }

        return Result.Success<Avatar?>(new Avatar(normalizedValue));
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
