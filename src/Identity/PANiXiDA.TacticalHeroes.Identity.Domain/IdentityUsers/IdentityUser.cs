using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityRoles;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.Events;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.Entities;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers;

public sealed class IdentityUser : AggregateRoot<IdentityUserId>
{
    private readonly List<IdentityUserRole> _roles = [];
    private readonly List<IdentityUserPermission> _permissions = [];

    private IdentityUser(
        IdentityUserId id,
        Email email,
        PasswordHash passwordHash,
        TokenHash? confirmationTokenHash,
        DateTimeOffset? confirmationTokenExpiresAtUtc)
        : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        ConfirmationTokenHash = confirmationTokenHash;
        ConfirmationTokenExpiresAtUtc = confirmationTokenExpiresAtUtc;
    }

    public Email Email { get; private set; }
    public PasswordHash PasswordHash { get; private set; }
    public bool IsConfirmed { get; private set; }
    public TokenHash? ConfirmationTokenHash { get; private set; }
    public DateTimeOffset? ConfirmationTokenExpiresAtUtc { get; private set; }
    public TokenHash? PasswordResetTokenHash { get; private set; }
    public DateTimeOffset? PasswordResetTokenExpiresAtUtc { get; private set; }
    public IReadOnlyCollection<IdentityUserRole> Roles => _roles;
    public IReadOnlyCollection<IdentityUserPermission> DirectPermissions => _permissions;

    public static Result<IdentityUser> Register(
        Email email,
        PasswordHash passwordHash,
        TokenHash confirmationTokenHash,
        DateTimeOffset confirmationTokenExpiresAtUtc,
        string confirmationToken)
    {
        if (confirmationTokenExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            return Result.Failure<IdentityUser>(
                Error.Validation("Confirmation token expiration must be in the future."));
        }

        var user = new IdentityUser(
            IdentityUserId.New(),
            email,
            passwordHash,
            confirmationTokenHash,
            confirmationTokenExpiresAtUtc);

        user.AddDomainEvent(
            new AccountConfirmationRequested(
                user.Id.Value,
                user.Email.Value,
                confirmationToken));

        return Result.Success(user);
    }

    public Result ConfirmRegistration(
        TokenHash confirmationTokenHash,
        DateTimeOffset confirmedAtUtc)
    {
        if (IsConfirmed)
        {
            return Result.Success();
        }

        if (ConfirmationTokenHash is null || ConfirmationTokenExpiresAtUtc is null)
        {
            return Result.Failure(
                Error.Conflict("Account confirmation was not requested."));
        }

        if (ConfirmationTokenExpiresAtUtc < confirmedAtUtc)
        {
            return Result.Failure(
                Error.Validation("Confirmation token expired."));
        }

        if (ConfirmationTokenHash != confirmationTokenHash)
        {
            return Result.Failure(
                Error.Validation("Confirmation token is invalid."));
        }

        IsConfirmed = true;
        ConfirmationTokenHash = null;
        ConfirmationTokenExpiresAtUtc = null;

        AddDomainEvent(
            new IdentityUserRegistered(
                Id.Value,
                Email.Value));

        return Result.Success();
    }

    public Result RequestPasswordReset(
        TokenHash passwordResetTokenHash,
        DateTimeOffset passwordResetTokenExpiresAtUtc,
        string passwordResetToken)
    {
        if (!IsConfirmed)
        {
            return Result.Failure(
                Error.Conflict("Cannot reset password for unconfirmed account."));
        }

        if (passwordResetTokenExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            return Result.Failure(
                Error.Validation("Password reset token expiration must be in the future."));
        }

        PasswordResetTokenHash = passwordResetTokenHash;
        PasswordResetTokenExpiresAtUtc = passwordResetTokenExpiresAtUtc;

        AddDomainEvent(
            new PasswordResetRequested(
                Id.Value,
                Email.Value,
                passwordResetToken));

        return Result.Success();
    }

    public Result ResetPassword(
        TokenHash passwordResetTokenHash,
        PasswordHash passwordHash,
        DateTimeOffset resetAtUtc)
    {
        if (PasswordResetTokenHash is null || PasswordResetTokenExpiresAtUtc is null)
        {
            return Result.Failure(
                Error.Conflict("Password reset was not requested."));
        }

        if (PasswordResetTokenExpiresAtUtc < resetAtUtc)
        {
            return Result.Failure(
                Error.Validation("Password reset token expired."));
        }

        if (PasswordResetTokenHash != passwordResetTokenHash)
        {
            return Result.Failure(
                Error.Validation("Password reset token is invalid."));
        }

        PasswordHash = passwordHash;
        PasswordResetTokenHash = null;
        PasswordResetTokenExpiresAtUtc = null;

        return Result.Success();
    }

    public Result AssignRole(IdentityRoleId roleId)
    {
        var roleResult = IdentityUserRole.Create(roleId);

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

    public Result GrantPermission(string permissionName)
    {
        var permissionResult = IdentityUserPermission.Create(permissionName);

        if (permissionResult.IsFailure)
        {
            return Result.Failure(permissionResult.Errors);
        }

        if (_permissions.Any(permission => permission.Name == permissionResult.Value.Name))
        {
            return Result.Success();
        }

        _permissions.Add(permissionResult.Value);

        return Result.Success();
    }
}
