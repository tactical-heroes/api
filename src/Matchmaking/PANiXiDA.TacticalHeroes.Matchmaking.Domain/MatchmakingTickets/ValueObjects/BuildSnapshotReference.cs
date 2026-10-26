namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.ValueObjects;

public sealed partial class BuildSnapshotReference : ValueObject
{
    private BuildSnapshotReference(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static Result<BuildSnapshotReference> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<BuildSnapshotReference>(Error.Validation("BuildSnapshotReference cannot be empty."))
            : Result.Success(new BuildSnapshotReference(value));
    }
}
