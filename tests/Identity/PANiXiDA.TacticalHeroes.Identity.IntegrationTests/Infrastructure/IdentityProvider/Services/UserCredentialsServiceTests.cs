using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Claims;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.IdentityProvider.Services;

public sealed class UserCredentialsServiceTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    private const string Password = "StrongPassword1!";

    [Fact(DisplayName = "RegisterAsync should persist an unconfirmed user")]
    public async Task RegisterAsync_Should_PersistUnconfirmedUser_When_CredentialsAreValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var scope = Fixture.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IUserCredentialsService>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var result = await service.RegisterAsync(
            " REGISTER@Example.COM ",
            " registered-hero ",
            Password,
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        var user = await userManager.FindByIdAsync(result.Value.ToString());
        user.ShouldNotBeNull();
        user.Email.ShouldBe("register@example.com");
        user.UserName.ShouldBe("registered-hero");
        user.EmailConfirmed.ShouldBeFalse();
    }

    [Fact(DisplayName = "LoginAsync should load user and all claims in one read query")]
    public async Task LoginAsync_Should_LoadUserAndAllClaimsInOneReadQuery()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var role = new ApplicationRole
        {
            Id = Guid.CreateVersion7(),
            Name = "admin",
            Claims =
            [
                new ApplicationRoleClaim
                {
                    ClaimType = "permission",
                    ClaimValue = "identity.users.manage"
                }
            ]
        };
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            Email = "login@example.com",
            UserName = "login-user",
            EmailConfirmed = true,
            Status = UserStatus.Active.Name,
            LockoutEnabled = true,
            Claims =
            [
                new ApplicationUserClaim
                {
                    ClaimType = "permission",
                    ClaimValue = "identity.profile.read"
                }
            ]
        };

        await AddAsync(role, user);
        Fixture.CommandCounter.Reset();

        await using var scope = Fixture.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IUserCredentialsService>();

        var result = await service.LoginAsync(
            user.Email,
            Password,
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Claims.ShouldContain(
            new Claim(type: "permission", value: "identity.profile.read"),
            IdentityClaimComparer.Instance);
        result.Value.Claims.ShouldContain(
            new Claim(type: "permission", value: "identity.users.manage"),
            IdentityClaimComparer.Instance);
        Fixture.CommandCounter.Count.ShouldBe(1);
    }

    [Fact(DisplayName = "Login should return unauthorized for an invalid password")]
    public async Task LoginAsync_Should_ReturnUnauthorized_When_PasswordIsInvalid()
    {
        var role = new ApplicationRole
        {
            Id = Guid.CreateVersion7(),
            Name = "login-role"
        };
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            Email = "invalid-password@example.com",
            UserName = "invalid-password-hero",
            EmailConfirmed = true,
            Status = UserStatus.Active.Name,
            LockoutEnabled = true
        };
        await AddAsync(role, user);

        await using var scope = Fixture.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IUserCredentialsService>();

        var result = await service.LoginAsync(
            user.Email,
            "WrongPassword1!",
            TestContext.Current.CancellationToken);

        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Unauthorized);
    }

    [Fact(DisplayName = "Change password should replace credentials stored by ASP.NET Core Identity")]
    public async Task ChangePasswordAsync_Should_ReplacePassword_When_CurrentPasswordIsValid()
    {
        var role = new ApplicationRole
        {
            Id = Guid.CreateVersion7(),
            Name = "password-role"
        };
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            Email = "change-password@example.com",
            UserName = "change-password-hero",
            EmailConfirmed = true,
            Status = UserStatus.Active.Name,
            LockoutEnabled = true
        };
        await AddAsync(role, user);

        await using var scope = Fixture.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IUserCredentialsService>();
        var cancellationToken = TestContext.Current.CancellationToken;

        var changeResult = await service.ChangePasswordAsync(
            user.Id,
            Password,
            "NewStrongPassword1!",
            cancellationToken);
        var loginResult = await service.LoginAsync(
            user.Email,
            "NewStrongPassword1!",
            cancellationToken);

        changeResult.IsSuccess.ShouldBeTrue();
        loginResult.IsSuccess.ShouldBeTrue();
        loginResult.Value.Id.ShouldBe(user.Id);
    }

    [Fact(DisplayName = "ConfirmEmailAsync should confirm an unconfirmed user")]
    public async Task ConfirmEmailAsync_Should_ConfirmUser_When_TokenIsValid()
    {
        var user = CreateUser(
            "confirm@example.com",
            "confirm-hero",
            isConfirmed: false);
        await AddUserAsync(user);

        await using var scope = Fixture.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var service = scope.ServiceProvider.GetRequiredService<IUserCredentialsService>();
        var persistedUser = await userManager.FindByIdAsync(user.Id.ToString());
        persistedUser.ShouldNotBeNull();
        var token = await userManager.GenerateEmailConfirmationTokenAsync(persistedUser);

        var result = await service.ConfirmEmailAsync(
            user.Id,
            token,
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        (await userManager.IsEmailConfirmedAsync(persistedUser)).ShouldBeTrue();
    }

    [Fact(DisplayName = "ResendConfirmationEmailAsync should track a confirmation request")]
    public async Task ResendConfirmationEmailAsync_Should_TrackRequest_When_UserIsUnconfirmed()
    {
        var user = CreateUser(
            "resend@example.com",
            "resend-hero",
            isConfirmed: false);
        await AddUserAsync(user);

        await using var scope = Fixture.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IUserCredentialsService>();
        var aggregateTracker = scope.ServiceProvider.GetRequiredService<IAggregateTracker>();

        var result = await service.ResendConfirmationEmailAsync(
            user.Email!,
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        var trackedUser = aggregateTracker.GetAll()
            .ShouldHaveSingleItem()
            .ShouldBeOfType<User>();
        var domainEvent = trackedUser.GetDomainEvents()
            .OfType<EmailConfirmationRequested>()
            .ShouldHaveSingleItem();
        domainEvent.Email.ShouldBe(user.Email);
        domainEvent.ConfirmationToken.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "ForgotPasswordAsync should track a password reset request")]
    public async Task ForgotPasswordAsync_Should_TrackRequest_When_UserIsConfirmed()
    {
        var user = CreateUser(
            "forgot@example.com",
            "forgot-hero",
            isConfirmed: true);
        await AddUserAsync(user);

        await using var scope = Fixture.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IUserCredentialsService>();
        var aggregateTracker = scope.ServiceProvider.GetRequiredService<IAggregateTracker>();

        var result = await service.ForgotPasswordAsync(
            user.Email!,
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        var trackedUser = aggregateTracker.GetAll()
            .ShouldHaveSingleItem()
            .ShouldBeOfType<User>();
        var domainEvent = trackedUser.GetDomainEvents()
            .OfType<PasswordResetRequested>()
            .ShouldHaveSingleItem();
        domainEvent.Email.ShouldBe(user.Email);
        domainEvent.PasswordResetToken.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "ResetPasswordAsync should replace a user's password")]
    public async Task ResetPasswordAsync_Should_ReplacePassword_When_TokenIsValid()
    {
        const string newPassword = "NewStrongPassword1!";
        var user = CreateUser(
            "reset@example.com",
            "reset-hero",
            isConfirmed: true);
        await AddUserAsync(user);

        await using var scope = Fixture.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var service = scope.ServiceProvider.GetRequiredService<IUserCredentialsService>();
        var persistedUser = await userManager.FindByIdAsync(user.Id.ToString());
        persistedUser.ShouldNotBeNull();
        var token = await userManager.GeneratePasswordResetTokenAsync(persistedUser);

        var result = await service.ResetPasswordAsync(
            user.Id,
            token,
            newPassword,
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        (await userManager.CheckPasswordAsync(persistedUser, Password)).ShouldBeFalse();
        (await userManager.CheckPasswordAsync(persistedUser, newPassword)).ShouldBeTrue();
    }

    private static ApplicationUser CreateUser(
        string email,
        string userName,
        bool isConfirmed)
    {
        return new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            Email = email,
            UserName = userName,
            EmailConfirmed = isConfirmed,
            Status = UserStatus.Active.Name,
            LockoutEnabled = true
        };
    }

    private async Task AddAsync(
        ApplicationRole role,
        ApplicationUser user)
    {
        await using var scope = Fixture.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        (await roleManager.CreateAsync(role: role)).Succeeded.ShouldBeTrue();
        (await userManager.CreateAsync(user: user, password: Password)).Succeeded.ShouldBeTrue();
        (await userManager.AddToRoleAsync(user, role.Name!)).Succeeded.ShouldBeTrue();
    }

    private async Task AddUserAsync(ApplicationUser user)
    {
        await using var scope = Fixture.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        (await userManager.CreateAsync(user: user, password: Password)).Succeeded.ShouldBeTrue();
    }
}
