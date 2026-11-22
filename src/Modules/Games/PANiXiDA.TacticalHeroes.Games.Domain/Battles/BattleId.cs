namespace PANiXiDA.TacticalHeroes.Games.Domain.Battles;

public readonly partial record struct BattleId : IStronglyTypedId
{
    private BattleId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static BattleId New()
    {
        return new BattleId(value: Guid.CreateVersion7());
    }

    public static Result<BattleId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<BattleId>(
                error: Error.Validation(message: "Battle id cannot be empty."))
            : Result.Success(value: new BattleId(value: value));
    }
}
