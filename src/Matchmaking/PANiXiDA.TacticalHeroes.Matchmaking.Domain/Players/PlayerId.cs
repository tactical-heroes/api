namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.Players;

public readonly partial record struct PlayerId : IStronglyTypedId
{
    private PlayerId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static PlayerId New()
    {
        return new PlayerId(Guid.CreateVersion7());
    }

    public static Result<PlayerId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<PlayerId>(Error.Validation("Player id cannot be empty."))
            : Result.Success(new PlayerId(value));
    }
}
