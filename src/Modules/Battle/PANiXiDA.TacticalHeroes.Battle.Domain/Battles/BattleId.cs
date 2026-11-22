namespace PANiXiDA.TacticalHeroes.Battle.Domain.Battles;

public readonly partial record struct BattleId : IStronglyTypedId
{
    private BattleId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static BattleId New()
    {
        return new BattleId(Guid.CreateVersion7());
    }

    public static Result<BattleId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<BattleId>(Error.Validation("Battle id cannot be empty."))
            : Result.Success(new BattleId(value));
    }
}
