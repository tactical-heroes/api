namespace PANiXiDA.TacticalHeroes.Builds.Domain.Players;

public readonly partial record struct PlayerId : IStronglyTypedId
{
    private PlayerId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static PlayerId New()
    {
        return new PlayerId(value: Guid.CreateVersion7());
    }

    public static Result<PlayerId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<PlayerId>(
                error: Error.Validation(message: "Player id cannot be empty."))
            : Result.Success(value: new PlayerId(value: value));
    }
}
