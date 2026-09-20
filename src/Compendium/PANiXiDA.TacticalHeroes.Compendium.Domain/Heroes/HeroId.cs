namespace PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;

public readonly record struct HeroId : IStronglyTypedId
{
    private HeroId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static HeroId New()
    {
        return new HeroId(value: Guid.CreateVersion7());
    }

    public static Result<HeroId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<HeroId>(
                error: Error.Validation(message: "Hero id cannot be empty."))
            : Result.Success(value: new HeroId(value: value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
