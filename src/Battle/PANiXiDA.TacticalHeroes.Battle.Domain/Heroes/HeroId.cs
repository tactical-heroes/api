namespace PANiXiDA.TacticalHeroes.Battle.Domain.Heroes;

public readonly partial record struct HeroId : IStronglyTypedId
{
    private HeroId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static HeroId New()
    {
        return new HeroId(Guid.CreateVersion7());
    }

    public static Result<HeroId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<HeroId>(Error.Validation("Hero definition id cannot be empty."))
            : Result.Success(new HeroId(value));
    }
}
