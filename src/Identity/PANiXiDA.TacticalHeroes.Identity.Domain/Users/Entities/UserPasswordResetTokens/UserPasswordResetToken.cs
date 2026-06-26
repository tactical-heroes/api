using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserPasswordResetTokens.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserPasswordResetTokens;

public sealed class UserPasswordResetToken : Entity<UserPasswordResetTokenId>
{
    private UserPasswordResetToken(
        UserId userId,
        PasswordResetTokenHash tokenHash,
        PasswordResetTokenExpiration expiresAtUtc)
        : this(UserPasswordResetTokenId.Create(userId).Value, tokenHash, expiresAtUtc)
    {
    }

    private UserPasswordResetToken(
        UserPasswordResetTokenId id,
        PasswordResetTokenHash tokenHash,
        PasswordResetTokenExpiration expiresAtUtc)
        : base(id)
    {
        UserId = id.UserId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
    }

    public UserId UserId { get; private set; }
    public PasswordResetTokenHash TokenHash { get; private set; }
    public PasswordResetTokenExpiration ExpiresAtUtc { get; private set; }

    internal static Result<UserPasswordResetToken> Create(
        UserId userId,
        string tokenHash,
        DateTimeOffset expiresAtUtc)
    {
        var idResult = UserPasswordResetTokenId.Create(userId);
        var tokenHashResult = PasswordResetTokenHash.Create(tokenHash);
        var expirationResult = PasswordResetTokenExpiration.CreateFuture(
            expiresAtUtc,
            DateTimeOffset.UtcNow);
        var validationResult = Result.Combine(idResult, tokenHashResult, expirationResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure<UserPasswordResetToken>(validationResult.Errors);
        }

        return Result.Success(new UserPasswordResetToken(
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
                Error.Validation("Password reset token expired."));
        }

        var tokenHashResult = PasswordResetTokenHash.Create(tokenHash);

        if (tokenHashResult.IsFailure)
        {
            return Result.Failure(tokenHashResult.Errors);
        }

        if (TokenHash != tokenHashResult.Value)
        {
            return Result.Failure(
                Error.Validation("Password reset token is invalid."));
        }

        return Result.Success();
    }
}
