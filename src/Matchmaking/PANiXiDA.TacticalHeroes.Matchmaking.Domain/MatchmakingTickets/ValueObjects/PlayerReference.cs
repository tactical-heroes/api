namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.ValueObjects;

public sealed partial class PlayerReference : ValueObject
{
    private PlayerReference(Guid value)
    {
        Value = value;
    }

    public Guid Value
    {
        get;
    }

    public static Result<PlayerReference> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<PlayerReference>(Error.Validation("PlayerReference cannot be empty."))
            : Result.Success(new PlayerReference(value));
    }
}
