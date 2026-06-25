namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users;

public readonly record struct UserId
{
    private UserId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static UserId New()
    {
        return new UserId(Guid.CreateVersion7());
    }

    public static Result<UserId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<UserId>(
                Error.Validation("User id cannot be empty."));
        }

        return Result.Success(new UserId(value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
