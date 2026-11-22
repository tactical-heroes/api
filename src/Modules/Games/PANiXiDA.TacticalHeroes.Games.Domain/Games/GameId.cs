namespace PANiXiDA.TacticalHeroes.Games.Domain.Games;

public readonly partial record struct GameId : IStronglyTypedId
{
    private GameId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static GameId New()
    {
        return new GameId(value: Guid.CreateVersion7());
    }

    public static Result<GameId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<GameId>(
                error: Error.Validation(message: "Game id cannot be empty."))
            : Result.Success(value: new GameId(value: value));
    }
}
