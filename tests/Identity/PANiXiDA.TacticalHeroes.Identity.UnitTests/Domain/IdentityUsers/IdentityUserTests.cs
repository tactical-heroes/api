using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityRoles;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.Events;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.IdentityUsers;

public sealed class IdentityUserTests
{
    [Fact(DisplayName = "Register should create unconfirmed user and raise confirmation requested event")]
    public void Register_Should_CreateUnconfirmedUser_And_RaiseConfirmationRequestedEvent()
    {
        var user = CreateUser("confirmation-token", out _);

        user.IsConfirmed.ShouldBeFalse();

        var confirmationEvent = user.GetDomainEvents()
            .OfType<AccountConfirmationRequested>()
            .Single();

        confirmationEvent.UserId.ShouldBe(user.Id.Value);
        confirmationEvent.Email.ShouldBe(user.Email.Value);
        confirmationEvent.ConfirmationToken.ShouldBe("confirmation-token");
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
        user.IsConfirmed.ShouldBeTrue();
        user.ConfirmationTokenHash.ShouldBeNull();
        user.ConfirmationTokenExpiresAtUtc.ShouldBeNull();

        var registeredEvent = user.GetDomainEvents()
            .OfType<IdentityUserRegistered>()
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

        var passwordResetTokenHash = TokenHash.Create("password-reset-token-hash").Value;
        var result = user.RequestPasswordReset(
            passwordResetTokenHash,
            DateTimeOffset.UtcNow.AddHours(1),
            "password-reset-token");

        result.IsSuccess.ShouldBeTrue();
        user.PasswordResetTokenHash.ShouldBe(passwordResetTokenHash);

        var passwordResetEvent = user.GetDomainEvents()
            .OfType<PasswordResetRequested>()
            .Single();

        passwordResetEvent.UserId.ShouldBe(user.Id.Value);
        passwordResetEvent.Email.ShouldBe(user.Email.Value);
        passwordResetEvent.PasswordResetToken.ShouldBe("password-reset-token");
    }

    [Fact(DisplayName = "Assign role should store role id once")]
    public void AssignRole_Should_StoreRoleIdOnce()
    {
        var user = CreateUser("confirmation-token", out _);
        var roleId = IdentityRoleId.New();

        var firstResult = user.AssignRole(roleId);
        var secondResult = user.AssignRole(roleId);

        firstResult.IsSuccess.ShouldBeTrue();
        secondResult.IsSuccess.ShouldBeTrue();
        user.Roles.Count.ShouldBe(1);
        user.Roles.Single().RoleId.ShouldBe(roleId);
    }

    private static IdentityUser CreateUser(
        string confirmationToken,
        out TokenHash confirmationTokenHash)
    {
        confirmationTokenHash = TokenHash.Create("confirmation-token-hash").Value;

        var userResult = IdentityUser.Register(
            Email.Create("hero@example.com").Value,
            PasswordHash.Create("password-hash").Value,
            confirmationTokenHash,
            DateTimeOffset.UtcNow.AddHours(1),
            confirmationToken);

        userResult.IsSuccess.ShouldBeTrue();

        return userResult.Value;
    }
}
