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

    public static Result<User> Register(string email)
    {
        var emailResult = Email.Create(email);

        if (emailResult.IsFailure)
        {
            return Result.Failure<User>(emailResult.Errors);
        }

        var user = new User(
            UserId.New(),
            emailResult.Value);

        return Result.Success(user);
    }

    internal static Result<User> Create(
        Guid id,
        string email,
        bool confirmationStatus,
        IEnumerable<Guid> roleIds,
        IEnumerable<(string Type, string Value)> claims)
    {
        var idResult = UserId.Create(id);
        var emailResult = Email.Create(email);
        var validationResult = Result.Combine(idResult, emailResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure<User>(validationResult.Errors);
        }

        var user = new User(
            idResult.Value,
            emailResult.Value)
        {
            ConfirmationStatus = UserConfirmationStatus.From(confirmationStatus)
        };

        foreach (var roleId in roleIds)
        {
            var assignRoleResult = user.AssignRole(roleId);

            if (assignRoleResult.IsFailure)
            {
                return Result.Failure<User>(assignRoleResult.Errors);
            }
        }

        foreach (var claim in claims)
        {
            var grantClaimResult = user.GrantClaim(claim.Type, claim.Value);

            if (grantClaimResult.IsFailure)
            {
                return Result.Failure<User>(grantClaimResult.Errors);
            }
        }

        return Result.Success(user);
    }

    public Result RequestAccountConfirmation(
        string confirmationToken,
        DateTimeOffset expiresAtUtc)
    {
        if (ConfirmationStatus.IsConfirmed)
        {
            return Result.Success();
        }

        AddDomainEvent(
            new AccountConfirmationRequested(
                Id.Value,
                Email.Value,
                confirmationToken,
                expiresAtUtc));

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
                Id.Value,
                Email.Value));

        return Result.Success();
    }

    public Result RequestPasswordReset(
        string passwordResetToken,
        DateTimeOffset expiresAtUtc)
    {
        if (!ConfirmationStatus.IsConfirmed)
        {
            return Result.Failure(
                Error.Conflict("Cannot reset password for unconfirmed account."));
        }

        AddDomainEvent(
            new PasswordResetRequested(
                Id.Value,
                Email.Value,
                passwordResetToken,
                expiresAtUtc));

        return Result.Success();
    }

    public Result AssignRole(Guid roleId)
    {
        var roleIdResult = RoleId.Create(roleId);

        if (roleIdResult.IsFailure)
        {
            return Result.Failure(roleIdResult.Errors);
        }

        if (_roleIds.Contains(roleIdResult.Value))
        {
            return Result.Success();
        }

        _roleIds.Add(roleIdResult.Value);

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
