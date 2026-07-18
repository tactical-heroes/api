namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users;

public readonly record struct UserId(Guid Value)
{
    public static UserId New()
    {
        return new UserId(Value: Guid.CreateVersion7());
    }

    public static Result<UserId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<UserId>(
                error: Error.Validation(message: "User id cannot be empty."));
        }

        return Result.Success(value: new UserId(Value: value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
