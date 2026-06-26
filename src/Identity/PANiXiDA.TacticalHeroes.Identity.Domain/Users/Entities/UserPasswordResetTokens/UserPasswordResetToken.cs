using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserPasswordResetTokens.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserPasswordResetTokens;

public sealed class UserPasswordResetToken : Entity<UserPasswordResetTokenId>
{
    private UserPasswordResetToken(
        UserId userId,
        PasswordResetTokenHash tokenHash,
        DateTimeOffset expiresAtUtc)
        : this(UserPasswordResetTokenId.Create(userId).Value, tokenHash, expiresAtUtc)
    {
    }

    private UserPasswordResetToken(
        UserPasswordResetTokenId id,
        PasswordResetTokenHash tokenHash,
        DateTimeOffset expiresAtUtc)
        : base(id)
    {
        UserId = id.UserId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
    }

    public UserId UserId { get; private set; }
    public PasswordResetTokenHash TokenHash { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private set; }

    internal static Result<UserPasswordResetToken> Create(
        UserId userId,
        string tokenHash,
        DateTimeOffset expiresAtUtc)
    {
        if (expiresAtUtc <= DateTimeOffset.UtcNow)
        {
            return Result.Failure<UserPasswordResetToken>(
                Error.Validation("Password reset token expiration must be in the future."));
        }

        var tokenHashResult = PasswordResetTokenHash.Create(tokenHash);

        if (tokenHashResult.IsFailure)
        {
            return Result.Failure<UserPasswordResetToken>(tokenHashResult.Errors);
        }

        return Result.Success(new UserPasswordResetToken(
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
