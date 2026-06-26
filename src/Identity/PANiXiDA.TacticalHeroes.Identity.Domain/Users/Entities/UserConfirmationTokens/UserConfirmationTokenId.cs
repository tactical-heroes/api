namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserConfirmationTokens;

public readonly record struct UserConfirmationTokenId
{
    private UserConfirmationTokenId(UserId userId)
    {
        UserId = userId;
    }

    public UserId UserId { get; }

    public static Result<UserConfirmationTokenId> Create(UserId userId)
    {
        if (userId.Value == Guid.Empty)
        {
            return Result.Failure<UserConfirmationTokenId>(
                Error.Validation("User id cannot be empty."));
        }

        return Result.Success(new UserConfirmationTokenId(userId));
    }

    public override string ToString()
    {
        return UserId.ToString();
    }
}
