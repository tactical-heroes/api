namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users;

public readonly partial record struct UserId : IStronglyTypedId
{
    private UserId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static UserId New()
    {
        return new UserId(value: Guid.CreateVersion7());
    }

    public static Result<UserId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<UserId>(
                error: Error.Validation(message: "User id cannot be empty."));
        }

        return Result.Success(value: new UserId(value: value));
    }
}
