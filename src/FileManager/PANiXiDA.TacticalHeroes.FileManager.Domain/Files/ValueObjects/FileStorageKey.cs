using System.Text;

namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

public sealed partial class FileStorageKey : ValueObject
{
    public const int MaxLength = 1024;

    private FileStorageKey(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<FileStorageKey> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<FileStorageKey>(
                Error.Validation("File storage key cannot be empty.")
                    .WithField(nameof(FileStorageKey)));
        }

        if (Encoding.UTF8.GetByteCount(value) > MaxLength)
        {
            return Result.Failure<FileStorageKey>(
                Error.Validation($"File storage key cannot be longer than {MaxLength} UTF-8 bytes.")
                    .WithField(nameof(FileStorageKey)));
        }

        if (value != value.Trim() ||
            value.Contains('\\') ||
            value.Any(char.IsControl) ||
            value.Split('/').Any(segment => segment is "" or "." or ".."))
        {
            return Result.Failure<FileStorageKey>(
                Error.Validation("File storage key must be a relative path without surrounding whitespace, empty or dot segments, backslashes, or control characters.")
                    .WithField(nameof(FileStorageKey)));
        }

        return Result.Success(new FileStorageKey(value));
    }
}
