namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserPasswordResetTokens;

public readonly record struct UserPasswordResetTokenId
{
    private UserPasswordResetTokenId(UserId userId)
    {
        UserId = userId;
    }

    public UserId UserId { get; }

    public static Result<UserPasswordResetTokenId> Create(UserId userId)
    {
        if (userId.Value == Guid.Empty)
        {
            return Result.Failure<UserPasswordResetTokenId>(
                Error.Validation("User id cannot be empty."));
        }

        return Result.Success(new UserPasswordResetTokenId(userId));
    }

    public override string ToString()
    {
        return UserId.ToString();
    }
}
