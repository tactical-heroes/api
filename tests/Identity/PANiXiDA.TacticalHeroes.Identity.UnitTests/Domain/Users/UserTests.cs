using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Users;

public sealed class UserTests
{
    [Fact(DisplayName = "Register should create an unconfirmed user with normalized email")]
    public void Register_Should_CreateUnconfirmedUser_When_EmailIsValid()
    {
        var result = User.Register(" HERO@Example.COM ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.Value.ShouldNotBe(Guid.Empty);
        result.Value.Email.Value.ShouldBe("hero@example.com");
        result.Value.ConfirmationStatus.IsConfirmed.ShouldBeFalse();
        result.Value.RoleIds.ShouldBeEmpty();
        result.Value.Claims.ShouldBeEmpty();
        result.Value.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Register should reject an invalid email")]
    public void Register_Should_ReturnValidationFailure_When_EmailIsInvalid()
    {
        var result = User.Register("invalid-email");

        result.ShouldHaveSingleError(ErrorType.Validation, "Email has invalid format.");
    }

    [Fact(DisplayName = "Create should restore persisted user state")]
    public void Create_Should_RestoreState_When_PersistedValuesAreValid()
    {
        var id = Guid.CreateVersion7();
        var roleId = Guid.CreateVersion7();

        var result = User.Create(
            id,
            "hero@example.com",
            true,
            [roleId],
            [("permission", "heroes.read")]);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.Value.ShouldBe(id);
        result.Value.ConfirmationStatus.IsConfirmed.ShouldBeTrue();
        result.Value.RoleIds.Single().Value.ShouldBe(roleId);
        result.Value.Claims.Single().Type.Value.ShouldBe("permission");
        result.Value.Claims.Single().Value.Value.ShouldBe("heroes.read");
    }

    [Fact(DisplayName = "Request email confirmation should raise an event for an unconfirmed user")]
    public void RequestEmailConfirmation_Should_RaiseEvent_When_UserIsUnconfirmed()
    {
        var user = CreateUser();
        var expiresAtUtc = DateTimeOffset.UtcNow.AddHours(24);

        var result = user.RequestEmailConfirmation("confirmation-token", expiresAtUtc);

        result.IsSuccess.ShouldBeTrue();
        var domainEvent = user.GetDomainEvents()
            .OfType<EmailConfirmationRequested>()
            .Single();
        domainEvent.UserId.ShouldBe(user.Id.Value);
        domainEvent.Email.ShouldBe(user.Email.Value);
        domainEvent.ConfirmationToken.ShouldBe("confirmation-token");
        domainEvent.ExpiresAtUtc.ShouldBe(expiresAtUtc);
    }

    [Fact(DisplayName = "Request email confirmation should not raise an event for a confirmed user")]
    public void RequestEmailConfirmation_Should_NotRaiseEvent_When_UserIsConfirmed()
    {
        var user = CreateUser();
        user.ConfirmRegistration();
        user.ClearDomainEvents();

        var result = user.RequestEmailConfirmation(
            "confirmation-token",
            DateTimeOffset.UtcNow.AddHours(24));

        result.IsSuccess.ShouldBeTrue();
        user.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Confirm registration should confirm the user and raise an event once")]
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

    [Fact(DisplayName = "Request password reset should reject an unconfirmed user")]
    public void RequestPasswordReset_Should_ReturnConflict_When_UserIsUnconfirmed()
    {
        var user = CreateUser();

        var result = user.RequestPasswordReset(
            "password-reset-token",
            DateTimeOffset.UtcNow.AddHours(1));

        result.ShouldHaveSingleError(
            ErrorType.Conflict,
            "Cannot reset password for unconfirmed user.");
        user.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Request password reset should raise an event for a confirmed user")]
    public void RequestPasswordReset_Should_RaiseEvent_When_UserIsConfirmed()
    {
        var user = CreateUser();
        user.ConfirmRegistration();
        user.ClearDomainEvents();
        var expiresAtUtc = DateTimeOffset.UtcNow.AddHours(1);

        var result = user.RequestPasswordReset("password-reset-token", expiresAtUtc);

        result.IsSuccess.ShouldBeTrue();
        var domainEvent = user.GetDomainEvents()
            .OfType<PasswordResetRequested>()
            .Single();
        domainEvent.UserId.ShouldBe(user.Id.Value);
        domainEvent.PasswordResetToken.ShouldBe("password-reset-token");
        domainEvent.ExpiresAtUtc.ShouldBe(expiresAtUtc);
    }

    [Fact(DisplayName = "Assign role should add a valid role only once")]
    public void AssignRole_Should_AddRoleOnce_When_RoleIdIsValid()
    {
        var user = CreateUser();
        var roleId = Guid.CreateVersion7();

        var firstResult = user.AssignRole(roleId);
        var secondResult = user.AssignRole(roleId);

        firstResult.IsSuccess.ShouldBeTrue();
        secondResult.IsSuccess.ShouldBeTrue();
        user.RoleIds.ShouldHaveSingleItem().Value.ShouldBe(roleId);
    }

    [Fact(DisplayName = "Assign role should reject an empty role id")]
    public void AssignRole_Should_ReturnValidationFailure_When_RoleIdIsEmpty()
    {
        var user = CreateUser();

        var result = user.AssignRole(Guid.Empty);

        result.ShouldHaveSingleError(ErrorType.Validation, "Role id cannot be empty.");
        user.RoleIds.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Grant claim should add a valid claim only once")]
    public void GrantClaim_Should_AddClaimOnce_When_ClaimIsValid()
    {
        var user = CreateUser();

        var firstResult = user.GrantClaim("permission", "heroes.read");
        var secondResult = user.GrantClaim("permission", "heroes.read");

        firstResult.IsSuccess.ShouldBeTrue();
        secondResult.IsSuccess.ShouldBeTrue();
        user.Claims.ShouldHaveSingleItem();
    }

    private static User CreateUser()
    {
        return User.Register("hero@example.com").Value;
    }
}
