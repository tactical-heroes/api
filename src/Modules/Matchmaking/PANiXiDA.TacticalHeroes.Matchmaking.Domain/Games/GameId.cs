namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.Games;

public readonly partial record struct GameId : IStronglyTypedId
{
    private GameId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static GameId New()
    {
        return new GameId(Guid.CreateVersion7());
    }

    public static Result<GameId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<GameId>(Error.Validation("Game id cannot be empty."))
            : Result.Success(new GameId(value));
    }
}
