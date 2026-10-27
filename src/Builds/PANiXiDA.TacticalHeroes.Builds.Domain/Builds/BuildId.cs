namespace PANiXiDA.TacticalHeroes.Builds.Domain.Builds;

public readonly partial record struct BuildId : IStronglyTypedId
{
    private BuildId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static BuildId New()
    {
        return new BuildId(value: Guid.CreateVersion7());
    }

    public static Result<BuildId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<BuildId>(
                error: Error.Validation(message: "Build id cannot be empty."))
            : Result.Success(value: new BuildId(value: value));
    }
}
