namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Entities.MatchParticipants;

public readonly partial record struct MatchParticipantId : IStronglyTypedId
{
    private MatchParticipantId(Guid value)
    {
        Value = value;
    }

    public Guid Value
    {
        get;
    }

    public static MatchParticipantId New()
    {
        return new MatchParticipantId(Guid.CreateVersion7());
    }

    public static Result<MatchParticipantId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<MatchParticipantId>(Error.Validation("MatchParticipant id cannot be empty."))
            : Result.Success(new MatchParticipantId(value));
    }
}
