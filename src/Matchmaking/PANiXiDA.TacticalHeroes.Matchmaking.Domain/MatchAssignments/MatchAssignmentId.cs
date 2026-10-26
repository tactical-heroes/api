namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments;

public readonly partial record struct MatchAssignmentId : IStronglyTypedId
{
    private MatchAssignmentId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static MatchAssignmentId New()
    {
        return new MatchAssignmentId(Guid.CreateVersion7());
    }

    public static Result<MatchAssignmentId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<MatchAssignmentId>(Error.Validation("MatchAssignment id cannot be empty."))
            : Result.Success(new MatchAssignmentId(value));
    }
}
