namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.ValueObjects;

public sealed partial class GameReference : ValueObject
{
    private GameReference(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static Result<GameReference> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<GameReference>(Error.Validation("GameReference cannot be empty."))
            : Result.Success(new GameReference(value));
    }
}
