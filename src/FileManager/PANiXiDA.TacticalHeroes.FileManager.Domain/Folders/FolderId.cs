namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;

public readonly partial record struct FolderId : IStronglyTypedId
{
    private FolderId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static FolderId New()
    {
        return new FolderId(value: Guid.CreateVersion7());
    }

    public static Result<FolderId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<FolderId>(
                error: Error.Validation(message: "Folder id cannot be empty."))
            : Result.Success(value: new FolderId(value: value));
    }
}
