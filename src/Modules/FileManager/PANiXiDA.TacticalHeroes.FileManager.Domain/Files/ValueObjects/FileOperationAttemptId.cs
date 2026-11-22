namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

public sealed partial class FileOperationAttemptId : ValueObject
{
    private FileOperationAttemptId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static FileOperationAttemptId New()
    {
        return new FileOperationAttemptId(value: Guid.CreateVersion7());
    }

    public static Result<FileOperationAttemptId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<FileOperationAttemptId>(
                error: Error.Validation(message: "File operation attempt id cannot be empty."))
            : Result.Success(value: new FileOperationAttemptId(value: value));
    }
}
