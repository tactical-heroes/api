namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Files;

public readonly partial record struct FileId : IStronglyTypedId
{
    private FileId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static FileId New()
    {
        return new FileId(value: Guid.CreateVersion7());
    }

    public static Result<FileId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<FileId>(
                error: Error.Validation(message: "File id cannot be empty."))
            : Result.Success(value: new FileId(value: value));
    }
}
