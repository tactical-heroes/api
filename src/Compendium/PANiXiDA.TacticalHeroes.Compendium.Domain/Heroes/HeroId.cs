namespace PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;

public readonly record struct HeroId(Guid Value) : IStronglyTypedId
{
    public static HeroId New()
    {
        return new HeroId(Value: Guid.CreateVersion7());
    }

    public static Result<HeroId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<HeroId>(
                error: Error.Validation(message: "Hero id cannot be empty."))
            : Result.Success(value: new HeroId(Value: value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
