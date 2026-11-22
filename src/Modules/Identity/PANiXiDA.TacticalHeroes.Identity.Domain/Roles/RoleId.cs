namespace PANiXiDA.TacticalHeroes.Identity.Domain.Roles;

public readonly partial record struct RoleId : IStronglyTypedId
{
    private RoleId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static RoleId New()
    {
        return new RoleId(value: Guid.CreateVersion7());
    }

    public static Result<RoleId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<RoleId>(
                error: Error.Validation(message: "Role id cannot be empty."));
        }

        return Result.Success(value: new RoleId(value: value));
    }

    public static Result<IReadOnlyCollection<RoleId>> Create(IReadOnlyCollection<Guid> values)
    {
        if (values is null)
        {
            return Result.Failure<IReadOnlyCollection<RoleId>>(
                error: Error.Validation(message: "Role IDs are required."));
        }

        var roleIds = new List<RoleId>();
        var seen = new HashSet<Guid>();
        foreach (var value in values)
        {
            if (!seen.Add(value))
            {
                return Result.Failure<IReadOnlyCollection<RoleId>>(
                    error: Error.Validation(message: "Role IDs must be unique."));
            }

            var result = Create(value);
            if (result.IsFailure)
            {
                return Result.Failure<IReadOnlyCollection<RoleId>>(errors: result.Errors);
            }

            roleIds.Add(result.Value);
        }

        return Result.Success<IReadOnlyCollection<RoleId>>(value: roleIds);
    }
}
