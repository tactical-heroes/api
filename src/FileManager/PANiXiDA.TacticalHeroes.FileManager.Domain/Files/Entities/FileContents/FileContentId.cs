namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Entities.FileContents;

public readonly partial record struct FileContentId : IStronglyTypedId
{
    private FileContentId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static FileContentId New()
    {
        return new FileContentId(Guid.CreateVersion7());
    }

    public static Result<FileContentId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<FileContentId>(Error.Validation("File content id cannot be empty."))
            : Result.Success(new FileContentId(value));
    }
}
