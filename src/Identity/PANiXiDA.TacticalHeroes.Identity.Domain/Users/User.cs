using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users;

public sealed class User : AggregateRoot<UserId>
{
    private readonly List<RoleId> _roleIds = [];
    private readonly List<UserClaim> _claims = [];

    private User(
        UserId id,
        Email email)
        : base(id)
    {
        Email = email;
        ConfirmationStatus = UserConfirmationStatus.Unconfirmed();
    }

    public Email Email { get; private set; }
    public UserConfirmationStatus ConfirmationStatus { get; private set; }

    public IReadOnlyCollection<RoleId> RoleIds => _roleIds;
    public IReadOnlyCollection<UserClaim> Claims => _claims;

    public static User Register(Email email)
    {
        return new User(id: UserId.New(), email: email);
    }

    public static User Create(
        UserId id,
        Email email,
        UserConfirmationStatus confirmationStatus,
        IEnumerable<RoleId> roleIds,
        IEnumerable<UserClaim> claims)
    {
        var user = new User(id: id, email: email)
        {
            ConfirmationStatus = confirmationStatus
        };

        foreach (var roleId in roleIds)
        {
            user.AssignRole(roleId);
        }

        foreach (var claim in claims)
        {
            user.GrantClaim(claim);
        }

        return user;
    }

    public Result RequestEmailConfirmation(
        UserActionToken confirmationToken)
    {
        if (ConfirmationStatus.IsConfirmed)
        {
            return Result.Success();
        }

        AddDomainEvent(
            new EmailConfirmationRequested(
                UserId: Id.Value,
                Email: Email.Value,
                ConfirmationToken: confirmationToken.Value,
                ExpiresAtUtc: confirmationToken.ExpiresAtUtc));

        return Result.Success();
    }

    public Result ConfirmRegistration()
    {
        if (ConfirmationStatus.IsConfirmed)
        {
            return Result.Success();
        }

        ConfirmationStatus = UserConfirmationStatus.Confirmed();

        AddDomainEvent(
            new UserRegistered(
                UserId: Id.Value,
                Email: Email.Value));

        return Result.Success();
    }

    public Result RequestPasswordReset(
        UserActionToken passwordResetToken)
    {
        if (!ConfirmationStatus.IsConfirmed)
        {
            return Result.Failure(
                error: Error.Conflict(message: "Cannot reset password for unconfirmed user."));
        }

        AddDomainEvent(
            new PasswordResetRequested(
                UserId: Id.Value,
                Email: Email.Value,
                PasswordResetToken: passwordResetToken.Value,
                ExpiresAtUtc: passwordResetToken.ExpiresAtUtc));

        return Result.Success();
    }

    public void AssignRole(RoleId roleId)
    {
        if (!_roleIds.Contains(roleId))
        {
            _roleIds.Add(roleId);
        }
    }

    public void GrantClaim(UserClaim claim)
    {
        if (_claims.Any(existing => existing.Type == claim.Type && existing.Value == claim.Value))
        {
            return;
        }

        _claims.Add(claim);
    }
}
