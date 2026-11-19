namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

public sealed partial class FileContentUpdateAttemptId : ValueObject
{
    private FileContentUpdateAttemptId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static FileContentUpdateAttemptId New()
    {
        return new FileContentUpdateAttemptId(value: Guid.CreateVersion7());
    }

    public static Result<FileContentUpdateAttemptId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<FileContentUpdateAttemptId>(
                error: Error.Validation(message: "File content update attempt id cannot be empty."))
            : Result.Success(value: new FileContentUpdateAttemptId(value: value));
    }
}
