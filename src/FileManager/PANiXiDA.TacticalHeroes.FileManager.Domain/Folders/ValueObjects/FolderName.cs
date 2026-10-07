namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;

public sealed partial class FolderName : ValueObject
{
    public const int MaxLength = 255;

    private FolderName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<FolderName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<FolderName>(
                Error.Validation("Folder name cannot be empty.")
                    .WithField(nameof(FolderName)));
        }

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > MaxLength)
        {
            return Result.Failure<FolderName>(
                Error.Validation($"Folder name cannot be longer than {MaxLength} characters.")
                    .WithField(nameof(FolderName)));
        }

        if (normalizedValue is "." or ".." ||
            normalizedValue.Contains('/') ||
            normalizedValue.Contains('\\') ||
            value.Any(char.IsControl))
        {
            return Result.Failure<FolderName>(
                Error.Validation("Folder name cannot contain paths or control characters.")
                    .WithField(nameof(FolderName)));
        }

        return Result.Success(new FolderName(normalizedValue));
    }
}
