namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims;

public readonly record struct UserClaimId
{
    private UserClaimId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static UserClaimId New()
    {
        return new UserClaimId(Guid.CreateVersion7());
    }

    public static Result<UserClaimId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<UserClaimId>(
                Error.Validation("User claim id cannot be empty."));
        }

        return Result.Success(new UserClaimId(value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
