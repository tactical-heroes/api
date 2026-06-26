using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserConfirmationTokens;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserPasswordResetTokens;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserRoles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users;

public sealed class User : AggregateRoot<UserId>
{
    private readonly List<UserRole> _roles = [];
    private readonly List<UserClaim> _claims = [];
    private UserConfirmationToken? _confirmationToken;
    private UserPasswordResetToken? _passwordResetToken;

    private User(
        UserId id,
        Email email,
        PasswordHash passwordHash)
        : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
    }

    public Email Email { get; private set; }
    public PasswordHash PasswordHash { get; private set; }
    public bool IsConfirmed { get; private set; }
    public UserConfirmationToken? ConfirmationToken => _confirmationToken;
    public UserPasswordResetToken? PasswordResetToken => _passwordResetToken;
    public IReadOnlyCollection<UserRole> Roles => _roles;
    public IReadOnlyCollection<UserClaim> Claims => _claims;

    public static Result<User> Register(
        Email email,
        PasswordHash passwordHash,
        string confirmationTokenHash,
        DateTimeOffset confirmationTokenExpiresAtUtc,
        string confirmationToken)
    {
        var user = new User(
            UserId.New(),
            email,
            passwordHash);

        var confirmationTokenResult = UserConfirmationToken.Create(
            user.Id,
            confirmationTokenHash,
            confirmationTokenExpiresAtUtc);

        if (confirmationTokenResult.IsFailure)
        {
            return Result.Failure<User>(
                confirmationTokenResult.Errors);
        }

        user._confirmationToken = confirmationTokenResult.Value;

        user.AddDomainEvent(
            new AccountConfirmationRequested(
                user.Id.Value,
                user.Email.Value,
                confirmationToken));

        return Result.Success(user);
    }

    public Result ConfirmRegistration(
        string confirmationTokenHash,
        DateTimeOffset confirmedAtUtc)
    {
        if (IsConfirmed)
        {
            return Result.Success();
        }

        if (_confirmationToken is null)
        {
            return Result.Failure(
                Error.Conflict("Account confirmation was not requested."));
        }

        var validationResult = _confirmationToken.Validate(
            confirmationTokenHash,
            confirmedAtUtc);

        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        IsConfirmed = true;
        _confirmationToken = null;

        AddDomainEvent(
            new UserRegistered(
                Id.Value,
                Email.Value));

        return Result.Success();
    }

    public Result RequestPasswordReset(
        string passwordResetTokenHash,
        DateTimeOffset passwordResetTokenExpiresAtUtc,
        string passwordResetToken)
    {
        if (!IsConfirmed)
        {
            return Result.Failure(
                Error.Conflict("Cannot reset password for unconfirmed account."));
        }

        var passwordResetTokenResult = UserPasswordResetToken.Create(
            Id,
            passwordResetTokenHash,
            passwordResetTokenExpiresAtUtc);

        if (passwordResetTokenResult.IsFailure)
        {
            return Result.Failure(
                passwordResetTokenResult.Errors);
        }

        _passwordResetToken = passwordResetTokenResult.Value;

        AddDomainEvent(
            new PasswordResetRequested(
                Id.Value,
                Email.Value,
                passwordResetToken));

        return Result.Success();
    }

    public Result ResetPassword(
        string passwordResetTokenHash,
        PasswordHash passwordHash,
        DateTimeOffset resetAtUtc)
    {
        if (_passwordResetToken is null)
        {
            return Result.Failure(
                Error.Conflict("Password reset was not requested."));
        }

        var validationResult = _passwordResetToken.Validate(
            passwordResetTokenHash,
            resetAtUtc);

        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        PasswordHash = passwordHash;
        _passwordResetToken = null;

        return Result.Success();
    }

    public Result AssignRole(RoleId roleId)
    {
        var roleResult = UserRole.Create(Id, roleId);

        if (roleResult.IsFailure)
        {
            return Result.Failure(roleResult.Errors);
        }

        if (_roles.Any(role => role.RoleId == roleResult.Value.RoleId))
        {
            return Result.Success();
        }

        _roles.Add(roleResult.Value);

        return Result.Success();
    }

    public Result GrantClaim(string type, string value)
    {
        var claimResult = UserClaim.Create(type, value);

        if (claimResult.IsFailure)
        {
            return Result.Failure(claimResult.Errors);
        }

        if (_claims.Any(claim =>
                claim.Type == claimResult.Value.Type &&
                claim.Value == claimResult.Value.Value))
        {
            return Result.Success();
        }

        _claims.Add(claimResult.Value);

        return Result.Success();
    }
}
