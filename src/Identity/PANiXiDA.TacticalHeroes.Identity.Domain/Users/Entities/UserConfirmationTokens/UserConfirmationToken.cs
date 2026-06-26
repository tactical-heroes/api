using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserConfirmationTokens.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserConfirmationTokens;

public sealed class UserConfirmationToken : Entity<UserConfirmationTokenId>
{
    private UserConfirmationToken(
        UserId userId,
        ConfirmationTokenHash tokenHash,
        DateTimeOffset expiresAtUtc)
        : this(UserConfirmationTokenId.Create(userId).Value, tokenHash, expiresAtUtc)
    {
    }

    private UserConfirmationToken(
        UserConfirmationTokenId id,
        ConfirmationTokenHash tokenHash,
        DateTimeOffset expiresAtUtc)
        : base(id)
    {
        UserId = id.UserId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
    }

    public UserId UserId { get; private set; }
    public ConfirmationTokenHash TokenHash { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private set; }

    internal static Result<UserConfirmationToken> Create(
        UserId userId,
        string tokenHash,
        DateTimeOffset expiresAtUtc)
    {
        if (expiresAtUtc <= DateTimeOffset.UtcNow)
        {
            return Result.Failure<UserConfirmationToken>(
                Error.Validation("Confirmation token expiration must be in the future."));
        }

        var tokenHashResult = ConfirmationTokenHash.Create(tokenHash);

        if (tokenHashResult.IsFailure)
        {
            return Result.Failure<UserConfirmationToken>(tokenHashResult.Errors);
        }

        return Result.Success(new UserConfirmationToken(
            userId,
            tokenHashResult.Value,
            expiresAtUtc));
    }

    internal Result Validate(
        string tokenHash,
        DateTimeOffset nowUtc)
    {
        if (ExpiresAtUtc < nowUtc)
        {
            return Result.Failure(
                Error.Validation("Confirmation token expired."));
        }

        var tokenHashResult = ConfirmationTokenHash.Create(tokenHash);

        if (tokenHashResult.IsFailure)
        {
            return Result.Failure(tokenHashResult.Errors);
        }

        if (TokenHash != tokenHashResult.Value)
        {
            return Result.Failure(
                Error.Validation("Confirmation token is invalid."));
        }

        return Result.Success();
    }
}
