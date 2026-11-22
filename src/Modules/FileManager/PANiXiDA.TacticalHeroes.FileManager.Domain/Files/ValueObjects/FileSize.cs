namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

public sealed partial class FileSize : ValueObject
{
    private FileSize(long value)
    {
        Value = value;
    }

    public long Value { get; }

    public static Result<FileSize> Create(long value)
    {
        if (value <= 0)
        {
            return Result.Failure<FileSize>(
                Error.Validation("File size must be greater than zero.")
                    .WithField(nameof(FileSize)));
        }

        return Result.Success(new FileSize(value));
    }
}
