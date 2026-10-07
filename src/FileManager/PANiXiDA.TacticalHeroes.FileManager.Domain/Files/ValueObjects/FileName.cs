namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

public sealed partial class FileName : ValueObject
{
    public const int MaxLength = 255;

    private FileName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<FileName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<FileName>(
                Error.Validation("File name cannot be empty.")
                    .WithField(nameof(FileName)));
        }

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > MaxLength)
        {
            return Result.Failure<FileName>(
                Error.Validation($"File name cannot be longer than {MaxLength} characters.")
                    .WithField(nameof(FileName)));
        }

        if (normalizedValue is "." or ".." ||
            normalizedValue.Contains('/') ||
            normalizedValue.Contains('\\') ||
            value.Any(char.IsControl))
        {
            return Result.Failure<FileName>(
                Error.Validation("File name cannot contain paths or control characters.")
                    .WithField(nameof(FileName)));
        }

        return Result.Success(new FileName(normalizedValue));
    }
}
