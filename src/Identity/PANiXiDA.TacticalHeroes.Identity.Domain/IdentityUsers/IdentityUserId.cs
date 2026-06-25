namespace PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers;

public readonly record struct IdentityUserId
{
    private IdentityUserId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static IdentityUserId New()
    {
        return new IdentityUserId(Guid.CreateVersion7());
    }

    public static Result<IdentityUserId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<IdentityUserId>(
                Error.Validation("Identity user id cannot be empty."));
        }

        return Result.Success(new IdentityUserId(value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
