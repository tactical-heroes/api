namespace PANiXiDA.TacticalHeroes.Games.Domain.Games.Entities.GameParticipants;

public readonly partial record struct GameParticipantId : IStronglyTypedId
{
    private GameParticipantId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static GameParticipantId New()
    {
        return new GameParticipantId(value: Guid.CreateVersion7());
    }

    public static Result<GameParticipantId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<GameParticipantId>(
                error: Error.Validation(message: "GameParticipant id cannot be empty."))
            : Result.Success(value: new GameParticipantId(value: value));
    }
}
