namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets;

public readonly partial record struct MatchmakingTicketId : IStronglyTypedId
{
    private MatchmakingTicketId(Guid value)
    {
        Value = value;
    }

    public Guid Value
    {
        get;
    }

    public static MatchmakingTicketId New()
    {
        return new MatchmakingTicketId(Guid.CreateVersion7());
    }

    public static Result<MatchmakingTicketId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<MatchmakingTicketId>(Error.Validation("MatchmakingTicket id cannot be empty."))
            : Result.Success(new MatchmakingTicketId(value));
    }
}
