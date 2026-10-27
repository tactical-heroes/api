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
        var roleId = RoleId.New();

        var result = User.Register(
            email: Email.Create(value: " HERO@Example.COM ").Value,
            userName: UserName.Create(value: " hero ").Value,
            roleId: roleId);

        result.Id.Value.ShouldNotBe(Guid.Empty);
        result.Email.Value.ShouldBe("hero@example.com");
        result.UserName.Value.ShouldBe("hero");
        result.Status.ShouldBe(UserStatus.Active);
        result.ConfirmationStatus.IsConfirmed.ShouldBeFalse();
        result.RoleIds.ShouldHaveSingleItem().ShouldBe(roleId);
        result.Claims.ShouldBeEmpty();
        result.GetDomainEvents().ShouldBeEmpty();
    }

    [Theory(
        DisplayName = "Restore should preserve persisted user state without events when persisted values are valid")]
    [InlineData(false)]
    [InlineData(true)]
    public void Restore_Should_PreserveStateWithoutEvents_When_PersistedValuesAreValid(bool isConfirmed)
    {
        var id = Guid.CreateVersion7();
        var roleId = Guid.CreateVersion7();

        var result = User.Restore(
            UserId.Create(id).Value,
            Email.Create("hero@example.com").Value,
            UserName.Create(" restored-hero ").Value,
            UserStatus.Blocked,
            UserConfirmationStatus.From(isConfirmed),
            [RoleId.Create(roleId).Value],
            [UserClaim.Create(ClaimType.Create("permission").Value, ClaimValue.Create("heroes.read").Value)]);

        result.Id.Value.ShouldBe(id);
        result.UserName.Value.ShouldBe("restored-hero");
        result.Status.ShouldBe(UserStatus.Blocked);
        result.ConfirmationStatus.IsConfirmed.ShouldBe(isConfirmed);
        result.RoleIds.Single().Value.ShouldBe(roleId);
        result.Claims.Single().Type.Value.ShouldBe("permission");
        result.Claims.Single().Value.Value.ShouldBe("heroes.read");
        result.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Create should raise registration exactly once when the new user is confirmed")]
    public void Create_Should_RaiseRegistrationOnce_When_UserIsConfirmed()
    {
        var id = UserId.New();

        var user = User.Create(
            id,
            Email.Create("hero@example.com").Value,
            UserName.Create("hero").Value,
            UserStatus.Active,
            UserConfirmationStatus.Confirmed(),
            [],
            []);

        user.ConfirmationStatus.IsConfirmed.ShouldBeTrue();
        var domainEvent = user.GetDomainEvents().ShouldHaveSingleItem().ShouldBeOfType<UserRegistered>();
        domainEvent.UserId.ShouldBe(id.Value);
        domainEvent.Email.ShouldBe(user.Email.Value);

        var confirmResult = user.ConfirmRegistration();

        confirmResult.IsSuccess.ShouldBeTrue();
        user.GetDomainEvents().ShouldHaveSingleItem().ShouldBeSameAs(domainEvent);
    }

    [Fact(DisplayName = "Create should not raise registration when the new user is unconfirmed")]
    public void Create_Should_NotRaiseRegistration_When_UserIsUnconfirmed()
    {
        var user = User.Create(
            UserId.New(),
            Email.Create("hero@example.com").Value,
            UserName.Create("hero").Value,
            UserStatus.Active,
            UserConfirmationStatus.Unconfirmed(),
            [],
            []);

        user.ConfirmationStatus.IsConfirmed.ShouldBeFalse();
        user.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Create should isolate collection storage when input collections change")]
    public void Create_Should_IsolateCollectionStorage_When_InputCollectionsChange()
    {
        var roleId = RoleId.New();
        var claim = UserClaim.Create(ClaimType.Create("permission").Value, ClaimValue.Create("heroes.read").Value);
        List<RoleId> roleIds = [roleId];
        List<UserClaim> claims = [claim];
        var user = User.Create(
            UserId.New(),
            Email.Create("hero@example.com").Value,
            UserName.Create("hero").Value,
            UserStatus.Active,
            UserConfirmationStatus.Unconfirmed(),
            roleIds,
            claims);

        roleIds.Clear();
        claims.Clear();

        user.RoleIds.ShouldHaveSingleItem().ShouldBe(roleId);
        user.Claims.ShouldHaveSingleItem().ShouldBe(claim);
    }

    [Theory(DisplayName = "Update should replace user state and register only when confirmation is granted")]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Update_Should_RegisterOnlyOnConfirmation_When_ConfirmationStateChanges(
        bool wasConfirmed,
        bool isConfirmed)
    {
        var id = UserId.New();
        var user = User.Restore(
            id,
            Email.Create("original@example.com").Value,
            UserName.Create("original-hero").Value,
            UserStatus.Active,
            UserConfirmationStatus.From(wasConfirmed),
            [RoleId.New()],
            [UserClaim.Create(ClaimType.Create("permission").Value, ClaimValue.Create("heroes.read").Value)]);
        var roleId = RoleId.New();
        var claim = UserClaim.Create(ClaimType.Create("permission").Value, ClaimValue.Create("heroes.manage").Value);
        List<RoleId> roleIds = [roleId, roleId];
        List<UserClaim> claims = [claim, claim];

        user.Update(
            Email.Create("updated@example.com").Value,
            UserName.Create("updated-hero").Value,
            UserStatus.Blocked,
            UserConfirmationStatus.From(isConfirmed),
            roleIds,
            claims);
        roleIds.Clear();
        claims.Clear();

        user.Id.ShouldBe(id);
        user.Email.Value.ShouldBe("updated@example.com");
        user.UserName.Value.ShouldBe("updated-hero");
        user.Status.ShouldBe(UserStatus.Blocked);
        user.ConfirmationStatus.IsConfirmed.ShouldBe(isConfirmed);
        user.RoleIds.ShouldHaveSingleItem().ShouldBe(roleId);
        user.Claims.ShouldHaveSingleItem().ShouldBe(claim);
        if (!wasConfirmed && isConfirmed)
        {
            var domainEvent = user.GetDomainEvents().ShouldHaveSingleItem().ShouldBeOfType<UserRegistered>();
            domainEvent.UserId.ShouldBe(id.Value);
            domainEvent.Email.ShouldBe("updated@example.com");
        }
        else
        {
            user.GetDomainEvents().ShouldBeEmpty();
        }
    }

    [Theory(DisplayName = "Update should preserve retained assignments or clear them when collections are replaced")]
    [InlineData(false)]
    [InlineData(true)]
    public void Update_Should_ReplaceAssignments_When_CollectionsAreProvided(bool retainAssignments)
    {
        var roleId = RoleId.New();
        var claim = UserClaim.Create(ClaimType.Create("permission").Value, ClaimValue.Create("heroes.read").Value);
        var user = User.Restore(
            UserId.New(),
            Email.Create("hero@example.com").Value,
            UserName.Create("hero").Value,
            UserStatus.Active,
            UserConfirmationStatus.Unconfirmed(),
            [roleId],
            [claim]);

        user.Update(
            user.Email,
            user.UserName,
            user.Status,
            user.ConfirmationStatus,
            retainAssignments ? user.RoleIds : [],
            retainAssignments ? user.Claims : []);

        if (retainAssignments)
        {
            user.RoleIds.ShouldHaveSingleItem().ShouldBe(roleId);
            user.Claims.ShouldHaveSingleItem().ShouldBe(claim);
        }
        else
        {
            user.RoleIds.ShouldBeEmpty();
            user.Claims.ShouldBeEmpty();
        }
        user.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact(
        DisplayName = "Request email confirmation should raise an event for an unconfirmed user when user is unconfirmed")]
    public void RequestEmailConfirmation_Should_RaiseEvent_When_UserIsUnconfirmed()
    {
        var user = CreateUser();
        var expiresAtUtc = DateTimeOffset.UtcNow.AddHours(24);

        var result = user.RequestEmailConfirmation(
            UserActionToken.Create(value: "confirmation-token", expiresAtUtc: expiresAtUtc).Value);

        result.IsSuccess.ShouldBeTrue();
        var domainEvent = user.GetDomainEvents()
            .OfType<EmailConfirmationRequested>()
            .Single();
        domainEvent.UserId.ShouldBe(user.Id.Value);
        domainEvent.Email.ShouldBe(user.Email.Value);
        domainEvent.ConfirmationToken.ShouldBe("confirmation-token");
        domainEvent.ExpiresAtUtc.ShouldBe(expiresAtUtc);
    }

    [Fact(
        DisplayName = "Request email confirmation should not raise an event for a confirmed user when user is confirmed")]
    public void RequestEmailConfirmation_Should_NotRaiseEvent_When_UserIsConfirmed()
    {
        var user = CreateUser();
        user.ConfirmRegistration();
        user.ClearDomainEvents();

        var result = user.RequestEmailConfirmation(
            UserActionToken.Create(value: "confirmation-token", expiresAtUtc: DateTimeOffset.UtcNow.AddHours(24))
                .Value);

        result.IsSuccess.ShouldBeTrue();
        user.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact(
        DisplayName = "Confirm registration should confirm the user and raise an event once when user is unconfirmed")]
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

        var result = user.RequestPasswordReset(
            UserActionToken.Create(value: "password-reset-token", expiresAtUtc: DateTimeOffset.UtcNow.AddHours(1))
                .Value);

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

        var result = user.RequestPasswordReset(
            UserActionToken.Create(value: "password-reset-token", expiresAtUtc: expiresAtUtc).Value);

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
        var roleIds = user.RoleIds;

        user.AssignRole(RoleId.Create(value: roleId).Value);
        user.AssignRole(RoleId.Create(value: roleId).Value);

        roleIds.ShouldHaveSingleItem().Value.ShouldBe(roleId);
    }

    [Fact(DisplayName = "Grant claim should add a valid claim only once when claim is valid")]
    public void GrantClaim_Should_AddClaimOnce_When_ClaimIsValid()
    {
        var user = CreateUser();
        var claims = user.Claims;

        user.GrantClaim(
            UserClaim.Create(
                ClaimType.Create(value: "permission").Value,
                ClaimValue.Create(value: "heroes.read").Value));
        user.GrantClaim(
            UserClaim.Create(
                ClaimType.Create(value: "permission").Value,
                ClaimValue.Create(value: "heroes.read").Value));

        claims.ShouldHaveSingleItem();
    }

    [Fact(DisplayName = "Role ids should reject external mutations when a role is assigned")]
    public void RoleIds_Should_RejectExternalMutations_When_ARoleIsAssigned()
    {
        var user = CreateUser();
        var roleId = RoleId.New();
        user.AssignRole(roleId);
        var collection = (IList<RoleId>)user.RoleIds;

        collection.IsReadOnly.ShouldBeTrue();
        Should.Throw<NotSupportedException>(() => collection.Add(RoleId.New()));
        Should.Throw<NotSupportedException>(() => collection.Remove(roleId));
        Should.Throw<NotSupportedException>(() => collection.Clear());
        Should.Throw<NotSupportedException>(() => collection[0] = RoleId.New());
        user.RoleIds.ShouldHaveSingleItem().ShouldBe(roleId);
    }

    [Fact(DisplayName = "Claims should reject external mutations when a claim is granted")]
    public void Claims_Should_RejectExternalMutations_When_AClaimIsGranted()
    {
        var user = CreateUser();
        var claim = UserClaim.Create(ClaimType.Create("permission").Value, ClaimValue.Create("heroes.read").Value);
        user.GrantClaim(claim);
        var collection = (IList<UserClaim>)user.Claims;

        collection.IsReadOnly.ShouldBeTrue();
        Should.Throw<NotSupportedException>(() => collection.Add(claim));
        Should.Throw<NotSupportedException>(() => collection.Remove(claim));
        Should.Throw<NotSupportedException>(() => collection.Clear());
        Should.Throw<NotSupportedException>(() => collection[0] = claim);
        user.Claims.ShouldHaveSingleItem().ShouldBe(claim);
    }

    private static User CreateUser()
    {
        return User.Create(
            id: UserId.New(),
            email: Email.Create(value: "hero@example.com").Value,
            userName: UserName.Create(value: " hero ").Value,
            status: UserStatus.Active,
            confirmationStatus: UserConfirmationStatus.Unconfirmed(),
            roleIds: [],
            claims: []);
    }
}
