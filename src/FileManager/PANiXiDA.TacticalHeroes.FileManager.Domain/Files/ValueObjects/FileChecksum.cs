namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

public sealed partial class FileChecksum : ValueObject
{
    public const int MaxLength = 64;

    private FileChecksum(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<FileChecksum> Create(string value)
    {
        return value is { Length: MaxLength } && value.All(char.IsAsciiHexDigit)
            ? Result.Success(new FileChecksum(value.ToLowerInvariant()))
            : Result.Failure<FileChecksum>(
                Error.Validation("File SHA-256 must contain exactly 64 hexadecimal characters.")
                    .WithField(nameof(FileChecksum)));
    }
}
