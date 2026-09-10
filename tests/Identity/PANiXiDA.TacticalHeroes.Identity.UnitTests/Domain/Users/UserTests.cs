using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Users;

public sealed class UserTests
{
    [Fact(DisplayName = "Register should create an unconfirmed user with normalized email when email is valid")]
    public void Register_Should_CreateUnconfirmedUser_When_EmailIsValid()
    {
        var result = User.Register(Email.Create(value: " HERO@Example.COM ").Value, UserName.Create(value: " hero ").Value);

        result.Id.Value.ShouldNotBe(Guid.Empty);
        result.Email.Value.ShouldBe("hero@example.com");
        result.UserName.Value.ShouldBe("hero");
        result.Status.ShouldBe(UserStatus.Active);
        result.ConfirmationStatus.IsConfirmed.ShouldBeFalse();
        result.RoleIds.ShouldBeEmpty();
        result.Claims.ShouldBeEmpty();
        result.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Create should restore persisted user state when persisted values are valid")]
    public void Create_Should_RestoreState_When_PersistedValuesAreValid()
    {
        var id = Guid.CreateVersion7();
        var roleId = Guid.CreateVersion7();

        var result = User.Create(
            UserId.Create(id).Value,
            Email.Create("hero@example.com").Value,
            UserName.Create(" restored-hero ").Value,
            UserStatus.Blocked,
            UserConfirmationStatus.Confirmed(),
            [RoleId.Create(roleId).Value],
            [UserClaim.Create(ClaimType.Create("permission").Value, ClaimValue.Create("heroes.read").Value)]);

        result.Id.Value.ShouldBe(id);
        result.UserName.Value.ShouldBe("restored-hero");
        result.Status.ShouldBe(UserStatus.Blocked);
        result.ConfirmationStatus.IsConfirmed.ShouldBeTrue();
        result.RoleIds.Single().Value.ShouldBe(roleId);
        result.Claims.Single().Type.Value.ShouldBe("permission");
        result.Claims.Single().Value.Value.ShouldBe("heroes.read");
    }

    [Fact(DisplayName = "Request email confirmation should raise an event for an unconfirmed user when user is unconfirmed")]
    public void RequestEmailConfirmation_Should_RaiseEvent_When_UserIsUnconfirmed()
    {
        var user = CreateUser();
        var expiresAtUtc = DateTimeOffset.UtcNow.AddHours(24);

        var result = user.RequestEmailConfirmation(UserActionToken.Create(value: "confirmation-token", expiresAtUtc: expiresAtUtc).Value);

        result.IsSuccess.ShouldBeTrue();
        var domainEvent = user.GetDomainEvents()
            .OfType<EmailConfirmationRequested>()
            .Single();
        domainEvent.UserId.ShouldBe(user.Id.Value);
        domainEvent.Email.ShouldBe(user.Email.Value);
        domainEvent.ConfirmationToken.ShouldBe("confirmation-token");
        domainEvent.ExpiresAtUtc.ShouldBe(expiresAtUtc);
    }

    [Fact(DisplayName = "Request email confirmation should not raise an event for a confirmed user when user is confirmed")]
    public void RequestEmailConfirmation_Should_NotRaiseEvent_When_UserIsConfirmed()
    {
        var user = CreateUser();
        user.ConfirmRegistration();
        user.ClearDomainEvents();

        var result = user.RequestEmailConfirmation(UserActionToken.Create(value: "confirmation-token", expiresAtUtc: DateTimeOffset.UtcNow.AddHours(24)).Value);

        result.IsSuccess.ShouldBeTrue();
        user.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Confirm registration should confirm the user and raise an event once when user is unconfirmed")]
    public void ConfirmRegistration_Should_RaiseEventOnce_When_UserIsUnconfirmed()
    {
        var user = CreateUser();

        var firstResult = user.ConfirmRegistration();
        var secondResult = user.ConfirmRegistration();

        firstResult.IsSuccess.ShouldBeTrue();
        secondResult.IsSuccess.ShouldBeTrue();
        user.ConfirmationStatus.IsConfirmed.ShouldBeTrue();
        var domainEvent = user.GetDomainEvents()
            .OfType<UserRegistered>()
            .ShouldHaveSingleItem();
        domainEvent.UserId.ShouldBe(user.Id.Value);
        domainEvent.Email.ShouldBe(user.Email.Value);
    }

    [Fact(DisplayName = "Request password reset should reject an unconfirmed user when user is unconfirmed")]
    public void RequestPasswordReset_Should_ReturnConflict_When_UserIsUnconfirmed()
    {
        var user = CreateUser();

        var result = user.RequestPasswordReset(UserActionToken.Create(value: "password-reset-token", expiresAtUtc: DateTimeOffset.UtcNow.AddHours(1)).Value);

        result.ShouldHaveSingleError(
            ErrorType.Conflict,
            "Cannot reset password for unconfirmed user.");
        user.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Request password reset should raise an event for a confirmed user when user is confirmed")]
    public void RequestPasswordReset_Should_RaiseEvent_When_UserIsConfirmed()
    {
        var user = CreateUser();
        user.ConfirmRegistration();
        user.ClearDomainEvents();
        var expiresAtUtc = DateTimeOffset.UtcNow.AddHours(1);

        var result = user.RequestPasswordReset(UserActionToken.Create(value: "password-reset-token", expiresAtUtc: expiresAtUtc).Value);

        result.IsSuccess.ShouldBeTrue();
        var domainEvent = user.GetDomainEvents()
            .OfType<PasswordResetRequested>()
            .Single();
        domainEvent.UserId.ShouldBe(user.Id.Value);
        domainEvent.PasswordResetToken.ShouldBe("password-reset-token");
        domainEvent.ExpiresAtUtc.ShouldBe(expiresAtUtc);
    }

    [Fact(DisplayName = "Assign role should add a valid role only once when role id is valid")]
    public void AssignRole_Should_AddRoleOnce_When_RoleIdIsValid()
    {
        var user = CreateUser();
        var roleId = Guid.CreateVersion7();

        user.AssignRole(RoleId.Create(value: roleId).Value);
        user.AssignRole(RoleId.Create(value: roleId).Value);

        user.RoleIds.ShouldHaveSingleItem().Value.ShouldBe(roleId);
    }

    [Fact(DisplayName = "Grant claim should add a valid claim only once when claim is valid")]
    public void GrantClaim_Should_AddClaimOnce_When_ClaimIsValid()
    {
        var user = CreateUser();

        user.GrantClaim(UserClaim.Create(ClaimType.Create(value: "permission").Value, ClaimValue.Create(value: "heroes.read").Value));
        user.GrantClaim(UserClaim.Create(ClaimType.Create(value: "permission").Value, ClaimValue.Create(value: "heroes.read").Value));

        user.Claims.ShouldHaveSingleItem();
    }

    private static User CreateUser()
    {
        return User.Register(Email.Create(value: "hero@example.com").Value, UserName.Create(value: " hero ").Value);
    }
}
