namespace PANiXiDA.TacticalHeroes.Builds.Domain.Builds.Entities.BuildHeroes;

public readonly partial record struct BuildHeroId : IStronglyTypedId
{
    private BuildHeroId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static BuildHeroId New()
    {
        return new BuildHeroId(value: Guid.CreateVersion7());
    }

    public static Result<BuildHeroId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<BuildHeroId>(
                error: Error.Validation(message: "BuildHero id cannot be empty."))
            : Result.Success(value: new BuildHeroId(value: value));
    }
}
