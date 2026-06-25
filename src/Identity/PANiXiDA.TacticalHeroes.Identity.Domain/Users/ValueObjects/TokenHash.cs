namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

public sealed class TokenHash : ValueObject
{
    public const int MaxLength = 128;

    private TokenHash(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<TokenHash> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<TokenHash>(
                Error.Validation("Token hash cannot be empty.")
                    .WithField(nameof(TokenHash)));
        }

        if (value.Length > MaxLength)
        {
            return Result.Failure<TokenHash>(
                Error.Validation($"Token hash cannot be longer than {MaxLength} characters.")
                    .WithField(nameof(TokenHash)));
        }

        return Result.Success(new TokenHash(value));
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
