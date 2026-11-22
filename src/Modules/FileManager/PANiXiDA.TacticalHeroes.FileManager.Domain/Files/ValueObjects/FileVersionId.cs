namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

public sealed partial class FileVersionId : ValueObject
{
    public const int MaxLength = 1024;

    private FileVersionId(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<FileVersionId> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "null")
        {
            return Result.Failure<FileVersionId>(
                Error.Validation("A versioned storage object is required.")
                    .WithField(nameof(FileVersionId)));
        }

        if (value.Length > MaxLength)
        {
            return Result.Failure<FileVersionId>(
                Error.Validation($"File version id cannot be longer than {MaxLength} characters.")
                    .WithField(nameof(FileVersionId)));
        }

        return Result.Success(new FileVersionId(value));
    }
}
