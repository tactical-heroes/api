namespace PANiXiDA.TacticalHeroes.Builds.Domain.Builds.Entities.BuildUnits;

public readonly partial record struct BuildUnitId : IStronglyTypedId
{
    private BuildUnitId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static BuildUnitId New()
    {
        return new BuildUnitId(value: Guid.CreateVersion7());
    }

    public static Result<BuildUnitId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<BuildUnitId>(
                error: Error.Validation(message: "BuildUnit id cannot be empty."))
            : Result.Success(value: new BuildUnitId(value: value));
    }
}
