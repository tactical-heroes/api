namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims;

public readonly record struct UserClaimId(Guid Value)
{
    public static UserClaimId New()
    {
        return new UserClaimId(Value: Guid.CreateVersion7());
    }

    public static Result<UserClaimId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<UserClaimId>(
                error: Error.Validation(message: "User claim id cannot be empty."));
        }

        return Result.Success(value: new UserClaimId(Value: value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
