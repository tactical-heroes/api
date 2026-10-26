namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.BuildSnapshots;

public readonly partial record struct BuildSnapshotId : IStronglyTypedId
{
    private BuildSnapshotId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static BuildSnapshotId New()
    {
        return new BuildSnapshotId(Guid.CreateVersion7());
    }

    public static Result<BuildSnapshotId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<BuildSnapshotId>(Error.Validation("BuildSnapshot id cannot be empty."))
            : Result.Success(new BuildSnapshotId(value));
    }
}
