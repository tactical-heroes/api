using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Users;

public sealed class UserTests
{
    [Fact(DisplayName = "Register should create unconfirmed user and raise confirmation requested event")]
    public void Register_Should_CreateUnconfirmedUser_And_RaiseConfirmationRequestedEvent()
    {
        var user = CreateUser("confirmation-token", out var confirmationTokenHash);

        user.ConfirmationStatus.IsConfirmed.ShouldBeFalse();
        user.ConfirmationToken.ShouldNotBeNull();
        user.ConfirmationToken.Id.UserId.ShouldBe(user.Id);
        user.ConfirmationToken.TokenHash.Value.ShouldBe(confirmationTokenHash);

        var confirmationEvent = user.GetDomainEvents()
            .OfType<AccountConfirmationRequested>()
            .Single();

        confirmationEvent.UserId.ShouldBe(user.Id.Value);
        confirmationEvent.Email.ShouldBe(user.Email.Value);
        confirmationEvent.ConfirmationToken.ShouldBe("confirmation-token");
        confirmationEvent.ExpiresAtUtc.ShouldBe(user.ConfirmationToken.ExpiresAtUtc.Value);
    }

    [Fact(DisplayName = "Confirm registration should confirm user and raise registered event")]
    public void ConfirmRegistration_Should_ConfirmUser_And_RaiseRegisteredEvent()
    {
        var user = CreateUser("confirmation-token", out var confirmationTokenHash);
        user.ClearDomainEvents();

        var result = user.ConfirmRegistration(
            confirmationTokenHash,
            DateTimeOffset.UtcNow);

        result.IsSuccess.ShouldBeTrue();
        user.ConfirmationStatus.IsConfirmed.ShouldBeTrue();
        user.ConfirmationToken.ShouldBeNull();

        var registeredEvent = user.GetDomainEvents()
            .OfType<UserRegistered>()
            .Single();

        registeredEvent.UserId.ShouldBe(user.Id.Value);
        registeredEvent.Email.ShouldBe(user.Email.Value);
    }

    [Fact(DisplayName = "Request password reset should require confirmed account and raise password reset event")]
    public void RequestPasswordReset_Should_RequireConfirmedAccount_And_RaisePasswordResetEvent()
    {
        var user = CreateUser("confirmation-token", out var confirmationTokenHash);
        user.ConfirmRegistration(confirmationTokenHash, DateTimeOffset.UtcNow);
        user.ClearDomainEvents();

        const string passwordResetTokenHash = "password-reset-token-hash";
        var result = user.RequestPasswordReset(
            passwordResetTokenHash,
            DateTimeOffset.UtcNow.AddHours(1),
            "password-reset-token");

        result.IsSuccess.ShouldBeTrue();
        user.PasswordResetToken.ShouldNotBeNull();
        user.PasswordResetToken.TokenHash.Value.ShouldBe(passwordResetTokenHash);

        var passwordResetEvent = user.GetDomainEvents()
            .OfType<PasswordResetRequested>()
            .Single();

        passwordResetEvent.UserId.ShouldBe(user.Id.Value);
        passwordResetEvent.Email.ShouldBe(user.Email.Value);
        passwordResetEvent.PasswordResetToken.ShouldBe("password-reset-token");
        passwordResetEvent.ExpiresAtUtc.ShouldBe(user.PasswordResetToken.ExpiresAtUtc.Value);
    }

    [Fact(DisplayName = "Assign role should store role id once")]
    public void AssignRole_Should_StoreRoleIdOnce()
    {
        var user = CreateUser("confirmation-token", out _);
        var roleId = RoleId.New();

        var firstResult = user.AssignRole(roleId.Value);
        var secondResult = user.AssignRole(roleId.Value);

        firstResult.IsSuccess.ShouldBeTrue();
        secondResult.IsSuccess.ShouldBeTrue();
        user.Roles.Count.ShouldBe(1);
        user.Roles.Single().Id.UserId.ShouldBe(user.Id);
        user.Roles.Single().Id.RoleId.ShouldBe(roleId);
        user.Roles.Single().RoleId.ShouldBe(roleId);
    }

    [Fact(DisplayName = "Grant claim should store claim type and value once")]
    public void GrantClaim_Should_StoreClaimTypeAndValueOnce()
    {
        var user = CreateUser("confirmation-token", out _);

        var firstResult = user.GrantClaim("permission", "identity.users.manage");
        var secondResult = user.GrantClaim("permission", "identity.users.manage");

        firstResult.IsSuccess.ShouldBeTrue();
        secondResult.IsSuccess.ShouldBeTrue();
        user.Claims.Count.ShouldBe(1);
        user.Claims.Single().Type.Value.ShouldBe("permission");
        user.Claims.Single().Value.Value.ShouldBe("identity.users.manage");
    }

    private static User CreateUser(
        string confirmationToken,
        out string confirmationTokenHash)
    {
        confirmationTokenHash = "confirmation-token-hash";

        var userResult = User.Register(
            "hero@example.com",
            "password-hash",
            confirmationTokenHash,
            DateTimeOffset.UtcNow.AddHours(1),
            confirmationToken);

        userResult.IsSuccess.ShouldBeTrue();

        return userResult.Value;
    }
}
