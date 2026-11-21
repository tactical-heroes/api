namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

public sealed partial class FileRelocationAttemptId : ValueObject
{
    private FileRelocationAttemptId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static FileRelocationAttemptId New()
    {
        return new FileRelocationAttemptId(value: Guid.CreateVersion7());
    }

    public static Result<FileRelocationAttemptId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<FileRelocationAttemptId>(
                error: Error.Validation(message: "File relocation attempt id cannot be empty."))
            : Result.Success(value: new FileRelocationAttemptId(value: value));
    }
}
