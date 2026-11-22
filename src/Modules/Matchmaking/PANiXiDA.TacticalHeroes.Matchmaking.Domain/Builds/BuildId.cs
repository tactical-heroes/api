namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.Builds;

public readonly partial record struct BuildId : IStronglyTypedId
{
    private BuildId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static BuildId New()
    {
        return new BuildId(Guid.CreateVersion7());
    }

    public static Result<BuildId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<BuildId>(Error.Validation("Build id cannot be empty."))
            : Result.Success(new BuildId(value));
    }
}
