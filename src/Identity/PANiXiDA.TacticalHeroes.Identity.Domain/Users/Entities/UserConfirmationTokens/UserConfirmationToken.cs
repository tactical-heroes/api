using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserConfirmationTokens.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserConfirmationTokens;

public sealed class UserConfirmationToken : Entity<UserConfirmationTokenId>
{
    private UserConfirmationToken(
        UserId userId,
        ConfirmationTokenHash tokenHash,
        ConfirmationTokenExpiration expiresAtUtc)
        : this(UserConfirmationTokenId.Create(userId).Value, tokenHash, expiresAtUtc)
    {
    }

    private UserConfirmationToken(
        UserConfirmationTokenId id,
        ConfirmationTokenHash tokenHash,
        ConfirmationTokenExpiration expiresAtUtc)
        : base(id)
    {
        UserId = id.UserId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
    }

    public UserId UserId { get; private set; }
    public ConfirmationTokenHash TokenHash { get; private set; }
    public ConfirmationTokenExpiration ExpiresAtUtc { get; private set; }

    internal static Result<UserConfirmationToken> Create(
        UserId userId,
        string tokenHash,
        DateTimeOffset expiresAtUtc)
    {
        var idResult = UserConfirmationTokenId.Create(userId);
        var tokenHashResult = ConfirmationTokenHash.Create(tokenHash);
        var expirationResult = ConfirmationTokenExpiration.CreateFuture(
            expiresAtUtc,
            DateTimeOffset.UtcNow);
        var validationResult = Result.Combine(idResult, tokenHashResult, expirationResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure<UserConfirmationToken>(validationResult.Errors);
        }

        return Result.Success(new UserConfirmationToken(
            idResult.Value,
            tokenHashResult.Value,
            expirationResult.Value));
    }

    internal Result Validate(
        string tokenHash,
        DateTimeOffset nowUtc)
    {
        if (ExpiresAtUtc.IsExpired(nowUtc))
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
