namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims;

public readonly partial record struct UserClaimId : IStronglyTypedId
{
    private UserClaimId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static UserClaimId New()
    {
        return new UserClaimId(value: Guid.CreateVersion7());
    }

    public static Result<UserClaimId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<UserClaimId>(
                error: Error.Validation(message: "User claim id cannot be empty."));
        }

        return Result.Success(value: new UserClaimId(value: value));
    }
}
