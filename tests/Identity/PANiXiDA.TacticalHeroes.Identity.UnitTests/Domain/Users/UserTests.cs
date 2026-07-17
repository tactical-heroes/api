using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Users;

public sealed class UserTests
{
    [Fact(DisplayName = "Register should create unconfirmed user")]
    public void Register_Should_CreateUnconfirmedUser()
    {
        var user = CreateUser();

        user.ConfirmationStatus.IsConfirmed.ShouldBeFalse();
        user.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Request account confirmation should raise confirmation requested event")]
    public void RequestAccountConfirmation_Should_RaiseConfirmationRequestedEvent()
    {
        var user = CreateUser();
        var expiresAtUtc = DateTimeOffset.UtcNow.AddHours(24);

        var result = user.RequestAccountConfirmation(
            "confirmation-token",
            expiresAtUtc);

        var confirmationEvent = user.GetDomainEvents()
            .OfType<AccountConfirmationRequested>()
            .Single();

        result.IsSuccess.ShouldBeTrue();
        confirmationEvent.UserId.ShouldBe(user.Id.Value);
        confirmationEvent.Email.ShouldBe(user.Email.Value);
        confirmationEvent.ConfirmationToken.ShouldBe("confirmation-token");
        confirmationEvent.ExpiresAtUtc.ShouldBe(expiresAtUtc);
    }

    [Fact(DisplayName = "Confirm registration should confirm user and raise registered event")]
    public void ConfirmRegistration_Should_ConfirmUser_And_RaiseRegisteredEvent()
    {
        var user = CreateUser();
        user.ClearDomainEvents();

        var result = user.ConfirmRegistration();

        result.IsSuccess.ShouldBeTrue();
        user.ConfirmationStatus.IsConfirmed.ShouldBeTrue();

        var registeredEvent = user.GetDomainEvents()
            .OfType<UserRegistered>()
            .Single();

        registeredEvent.UserId.ShouldBe(user.Id.Value);
        registeredEvent.Email.ShouldBe(user.Email.Value);
    }

    [Fact(DisplayName = "Request password reset should require confirmed account and raise password reset event")]
    public void RequestPasswordReset_Should_RequireConfirmedAccount_And_RaisePasswordResetEvent()
    {
        var user = CreateUser();
        var expiresAtUtc = DateTimeOffset.UtcNow.AddHours(1);
        user.ConfirmRegistration();
        user.ClearDomainEvents();

        var result = user.RequestPasswordReset(
            "password-reset-token",
            expiresAtUtc);

        result.IsSuccess.ShouldBeTrue();

        var passwordResetEvent = user.GetDomainEvents()
            .OfType<PasswordResetRequested>()
            .Single();

        passwordResetEvent.UserId.ShouldBe(user.Id.Value);
        passwordResetEvent.Email.ShouldBe(user.Email.Value);
        passwordResetEvent.PasswordResetToken.ShouldBe("password-reset-token");
        passwordResetEvent.ExpiresAtUtc.ShouldBe(expiresAtUtc);
    }

    [Fact(DisplayName = "Assign role should store role id once")]
    public void AssignRole_Should_StoreRoleIdOnce()
    {
        var user = CreateUser();
        var roleId = RoleId.New();

        var firstResult = user.AssignRole(roleId.Value);
        var secondResult = user.AssignRole(roleId.Value);

        firstResult.IsSuccess.ShouldBeTrue();
        secondResult.IsSuccess.ShouldBeTrue();
        user.RoleIds.Count.ShouldBe(1);
        user.RoleIds.Single().ShouldBe(roleId);
    }

    [Fact(DisplayName = "Grant claim should store claim type and value once")]
    public void GrantClaim_Should_StoreClaimTypeAndValueOnce()
    {
        var user = CreateUser();

        var firstResult = user.GrantClaim("permission", "identity.users.manage");
        var secondResult = user.GrantClaim("permission", "identity.users.manage");

        firstResult.IsSuccess.ShouldBeTrue();
        secondResult.IsSuccess.ShouldBeTrue();
        user.Claims.Count.ShouldBe(1);
        user.Claims.Single().Type.Value.ShouldBe("permission");
        user.Claims.Single().Value.Value.ShouldBe("identity.users.manage");
    }

    private static User CreateUser()
    {
        var userResult = User.Register(
            email: "hero@example.com");

        userResult.IsSuccess.ShouldBeTrue();

        return userResult.Value;
    }
}
