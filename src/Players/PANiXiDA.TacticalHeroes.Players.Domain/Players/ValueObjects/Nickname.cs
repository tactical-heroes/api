namespace PANiXiDA.TacticalHeroes.Players.Domain.Players.ValueObjects;

public sealed partial class Nickname : ValueObject
{
    public const int MaxLength = 32;

    private Nickname(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Nickname> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<Nickname>(
                error: Error.Validation(message: "Nickname cannot be empty.")
                    .WithField(nameof(Nickname)));
        }

        var normalizedValue = value.Trim();

        return normalizedValue.Length <= MaxLength
            ? Result.Success(value: new Nickname(value: normalizedValue))
            : Result.Failure<Nickname>(
                error: Error.Validation(message: $"Nickname cannot be longer than {MaxLength} characters.")
                    .WithField(nameof(Nickname)));
    }
}
