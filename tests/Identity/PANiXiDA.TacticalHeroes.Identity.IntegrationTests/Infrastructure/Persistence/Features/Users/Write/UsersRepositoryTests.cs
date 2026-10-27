using System.Security.Claims;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Users;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Features.Users.Write;

public sealed class UsersRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    private const string Password = "StrongPassword1!";

    [Theory(DisplayName = "Loaded users should register only when confirmation is granted during an update")]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task UpdateAsync_Should_TrackRegistrationOnlyOnConfirmation_When_LoadedUserIsUpdated(
        bool wasConfirmed,
        bool isConfirmed)
    {
        var ct = TestContext.Current.CancellationToken;
        var id = await AddUserAsync(
            "hero@example.com", "hero", wasConfirmed, [], UserStatus.Active.Name, ct);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
            var tracker = scope.ServiceProvider.GetRequiredService<IAggregateTracker>();
            var loaded = await repository.GetByIdAsync(UserId.Create(id).Value, ct);
            loaded.IsSuccess.ShouldBeTrue();
            var user = loaded.Value;
            user.ConfirmationStatus.IsConfirmed.ShouldBe(wasConfirmed);
            user.GetDomainEvents().ShouldBeEmpty();
            tracker.GetAll().ShouldBeEmpty();

            user.Update(
                Email.Create("updated@example.com").Value,
                UserName.Create("updated-hero").Value,
                UserStatus.Active,
                UserConfirmationStatus.From(isConfirmed),
                [],
                []);
            var result = await repository.UpdateAsync(user, null, ct);

            result.IsSuccess.ShouldBeTrue();
            tracker.GetAll().ShouldHaveSingleItem().ShouldBeSameAs(user);
            if (!wasConfirmed && isConfirmed)
            {
                var domainEvent = user.GetDomainEvents().ShouldHaveSingleItem().ShouldBeOfType<UserRegistered>();
                domainEvent.UserId.ShouldBe(id);
                domainEvent.Email.ShouldBe("updated@example.com");
            }
            else
            {
                user.GetDomainEvents().ShouldBeEmpty();
            }
        }

        await using var verificationScope = Fixture.CreateScope();
        var verificationRepository = verificationScope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var persisted = await verificationRepository.GetByIdAsync(UserId.Create(id).Value, ct);
        persisted.IsSuccess.ShouldBeTrue();
        persisted.Value.Email.Value.ShouldBe("updated@example.com");
        persisted.Value.UserName.Value.ShouldBe("updated-hero");
        persisted.Value.ConfirmationStatus.IsConfirmed.ShouldBe(isConfirmed);
        persisted.Value.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact(DisplayName = "GetByIdAsync should return not found when the user does not exist")]
    public async Task GetByIdAsync_Should_ReturnNotFound_When_UserDoesNotExist()
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();

        var result = await repository.GetByIdAsync(UserId.New(), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.NotFound);
    }

    [Fact(DisplayName = "AddAsync should persist a valid user when command is valid")]
    public async Task AddAsync_Should_PersistUser_When_CommandIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = await AddUserAsync(
            " HERO@Example.COM ",
            " hero ",
            isConfirmed: false,
            [new Claim("permission", "heroes.read")],
            UserStatus.Active.Name,
            cancellationToken);

        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var user = await dbContext.Set<ApplicationUser>()
            .Include(item => item.Claims)
            .AsNoTracking()
            .SingleAsync(item => item.Id == userId, cancellationToken);

        user.Email.ShouldBe("hero@example.com");
        user.UserName.ShouldBe("hero");
        user.EmailConfirmed.ShouldBeFalse();
        user.Status.ShouldBe(UserStatus.Active.Name);
        user.Claims.ShouldHaveSingleItem().ClaimValue.ShouldBe("heroes.read");
    }

    [Fact(DisplayName = "UpdateAsync should persist user changes when user exists")]
    public async Task UpdateAsync_Should_PersistChanges_When_UserExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = await AddUserAsync(
            "original@example.com",
            "original-hero",
            isConfirmed: false,
            [],
            UserStatus.Active.Name,
            cancellationToken);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
            var result = await repository.UpdateAsync(
                user: IntegrationTestData.CreateUser(
                    id: userId,
                    email: "updated@example.com",
                    isConfirmed: true,
                    roleIds: [],
                    claims: ((Claim[])[new Claim("permission", "heroes.manage")]).Select(
                        claim => (claim.Type, claim.Value)),
                    userName: "updated-hero",
                    status: UserStatus.Blocked),
                currentAccessToken: null,
                cancellationToken: cancellationToken);

            result.IsSuccess.ShouldBeTrue();
        }

        await using var verificationScope = Fixture.CreateScope();
        var dbContext =
            verificationScope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var user = await dbContext.Set<ApplicationUser>()
            .Include(item => item.Claims)
            .AsNoTracking()
            .SingleAsync(item => item.Id == userId, cancellationToken);

        user.Email.ShouldBe("updated@example.com");
        user.UserName.ShouldBe("updated-hero");
        user.EmailConfirmed.ShouldBeTrue();
        user.Status.ShouldBe(UserStatus.Blocked.Name);
        user.Claims.ShouldHaveSingleItem().ClaimValue.ShouldBe("heroes.manage");
    }

    [Fact(DisplayName = "DeleteAsync should remove an existing user when user exists")]
    public async Task DeleteAsync_Should_RemoveUser_When_UserExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = await AddUserAsync(
            "delete@example.com",
            "delete-hero",
            isConfirmed: true,
            [],
            UserStatus.Active.Name,
            cancellationToken);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
            (await repository.DeleteAsync(userId, cancellationToken)).IsSuccess.ShouldBeTrue();
        }

        await using var verificationScope = Fixture.CreateScope();
        var dbContext =
            verificationScope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        (await dbContext.Set<ApplicationUser>()
            .AnyAsync(item => item.Id == userId, cancellationToken))
            .ShouldBeFalse();
    }

    [Fact(DisplayName = "Users write repository should return not found for a missing user when user does not exist")]
    public async Task DeleteAsync_Should_ReturnNotFound_When_UserDoesNotExist()
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();

        var result = await repository.DeleteAsync(
            Guid.CreateVersion7(),
            TestContext.Current.CancellationToken);

        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.NotFound);
    }

    [Fact(DisplayName = "BlockAsync should block an active user when user is active")]
    public async Task BlockAsync_Should_BlockUser_When_UserIsActive()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = await AddUserAsync(
            "block@example.com",
            "block-hero",
            isConfirmed: true,
            [],
            UserStatus.Active.Name,
            cancellationToken);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
            (await repository.BlockAsync(userId, cancellationToken)).IsSuccess.ShouldBeTrue();
        }

        (await ReadStatusAsync(userId, cancellationToken)).ShouldBe(UserStatus.Blocked.Name);
    }

    [Theory(DisplayName = "User blocking should invalidate sessions when block entry points vary")]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task UserBlocking_Should_InvalidateSessions_When_BlockEntryPointsVary(
        bool generalUpdate, bool rollback)
    {
        var ct = TestContext.Current.CancellationToken;
        var id = await AddUserAsync("hero@example.com", "hero", true, [], UserStatus.Active.Name, ct);
        var otherId = await AddUserAsync("other@example.com", "other", true, [], UserStatus.Active.Name, ct);
        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var authorizations = scope.ServiceProvider.GetRequiredService<IOpenIddictAuthorizationManager>();
        var tokens = scope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();
        var originalStamps = await dbContext.Users.AsNoTracking()
            .ToDictionaryAsync(user => user.Id, user => user.SecurityStamp, ct);
        foreach (var userId in new[] { id, otherId })
        {
            var authorization = await authorizations.CreateAsync(
                new OpenIddictAuthorizationDescriptor
                {
                    Subject = userId.ToString(),
                    Type = OpenIddictConstants.AuthorizationTypes.AdHoc,
                    Status = OpenIddictConstants.Statuses.Valid
                },
                ct);
            await tokens.CreateAsync(
                new OpenIddictTokenDescriptor
                {
                    Subject = userId.ToString(),
                    AuthorizationId = await authorizations.GetIdAsync(authorization, ct),
                    Type = OpenIddictConstants.TokenTypeIdentifiers.RefreshToken,
                    Status = OpenIddictConstants.Statuses.Valid
                },
                ct);
        }
        await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);

        var result = generalUpdate
            ? await repository.UpdateAsync(
                user: IntegrationTestData.CreateUser(
                    id: id,
                    email: "hero@example.com",
                    isConfirmed: true,
                    roleIds: [],
                    claims: [],
                    userName: "hero",
                    status: UserStatus.Blocked),
                currentAccessToken: null,
                cancellationToken: ct)
            : await repository.BlockAsync(id, ct);

        result.IsSuccess.ShouldBeTrue();
        var blocked = await dbContext.Users.AsNoTracking().SingleAsync(user => user.Id == id, ct);
        blocked.Status.ShouldBe(UserStatus.Blocked.Name);
        blocked.SecurityStamp.ShouldNotBe(originalStamps[id]);
        if (rollback)
        {
            await transaction.RollbackAsync(ct);
        }
        else
        {
            await transaction.CommitAsync(ct);
        }
        await using var verification = Fixture.CreateScope();
        var persisted = verification.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var users = await persisted.Users.AsNoTracking().ToDictionaryAsync(user => user.Id, ct);
        users[id].Status.ShouldBe(rollback ? UserStatus.Active.Name : UserStatus.Blocked.Name);
        users[id].SecurityStamp.ShouldBe(rollback ? originalStamps[id] : blocked.SecurityStamp);
        users[otherId].SecurityStamp.ShouldBe(originalStamps[otherId]);
        var storedAuthorizations = await persisted.Set<OpenIddictEntityFrameworkCoreAuthorization<Guid>>()
            .AsNoTracking()
            .ToListAsync(ct);
        var storedTokens = await persisted.Set<OpenIddictEntityFrameworkCoreToken<Guid>>()
            .AsNoTracking()
            .ToListAsync(ct);
        var expected = rollback ? OpenIddictConstants.Statuses.Valid : OpenIddictConstants.Statuses.Revoked;
        storedAuthorizations.Single(item => item.Subject == id.ToString()).Status.ShouldBe(expected);
        storedTokens.Single(item => item.Subject == id.ToString()).Status.ShouldBe(expected);
        storedAuthorizations.Single(item => item.Subject == otherId.ToString()).Status
            .ShouldBe(OpenIddictConstants.Statuses.Valid);
        storedTokens.Single(item => item.Subject == otherId.ToString()).Status
            .ShouldBe(OpenIddictConstants.Statuses.Valid);
    }

    [Fact(DisplayName = "UpdateAsync should preserve sessions when block only update fails validation")]
    public async Task UpdateAsync_Should_PreserveSessions_When_BlockOnlyUpdateFailsValidation()
    {
        var ct = TestContext.Current.CancellationToken;
        var id = await AddUserAsync("hero@example.com", "hero", true, [], UserStatus.Active.Name, ct);
        await AddUserAsync("occupied@example.com", "occupied", true, [], UserStatus.Active.Name, ct);
        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var authorizations = scope.ServiceProvider.GetRequiredService<IOpenIddictAuthorizationManager>();
        var tokens = scope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();
        var before = await dbContext.Users.AsNoTracking().SingleAsync(user => user.Id == id, ct);
        var authorization = await authorizations.CreateAsync(
            new OpenIddictAuthorizationDescriptor
            {
                Subject = id.ToString(),
                Type = OpenIddictConstants.AuthorizationTypes.AdHoc,
                Status = OpenIddictConstants.Statuses.Valid
            },
            ct);
        await tokens.CreateAsync(
            new OpenIddictTokenDescriptor
            {
                Subject = id.ToString(),
                AuthorizationId = await authorizations.GetIdAsync(authorization, ct),
                Type = OpenIddictConstants.TokenTypeIdentifiers.RefreshToken,
                Status = OpenIddictConstants.Statuses.Valid
            },
            ct);

        var result = await repository.UpdateAsync(
            user: IntegrationTestData.CreateUser(
                id: id,
                email: "hero@example.com",
                isConfirmed: true,
                roleIds: [],
                claims: [],
                userName: "occupied",
                status: UserStatus.Blocked),
            currentAccessToken: null,
            cancellationToken: ct);

        result.Errors.ShouldContain(error => error.Type == ErrorType.Conflict);
        await using var verification = Fixture.CreateScope();
        var persisted = verification.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var user = await persisted.Users.AsNoTracking().SingleAsync(user => user.Id == id, ct);
        user.Status.ShouldBe(UserStatus.Active.Name);
        user.SecurityStamp.ShouldBe(before.SecurityStamp);
        var persistedAuthorization = await persisted.Set<OpenIddictEntityFrameworkCoreAuthorization<Guid>>()
            .AsNoTracking()
            .SingleAsync(ct);
        var persistedToken = await persisted.Set<OpenIddictEntityFrameworkCoreToken<Guid>>()
            .AsNoTracking()
            .SingleAsync(ct);
        persistedAuthorization.Status.ShouldBe(OpenIddictConstants.Statuses.Valid);
        persistedToken.Status.ShouldBe(OpenIddictConstants.Statuses.Valid);
    }

    [Fact(DisplayName = "UnblockAsync should activate a blocked user when user is blocked")]
    public async Task UnblockAsync_Should_ActivateUser_When_UserIsBlocked()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = await AddUserAsync(
            "unblock@example.com",
            "unblock-hero",
            isConfirmed: true,
            [],
            UserStatus.Blocked.Name,
            cancellationToken);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
            (await repository.UnblockAsync(userId, cancellationToken)).IsSuccess.ShouldBeTrue();
        }

        (await ReadStatusAsync(userId, cancellationToken)).ShouldBe(UserStatus.Active.Name);
    }

    [Theory(DisplayName = "User updates should synchronize roles when role assignments vary")]
    [InlineData("same")]
    [InlineData("replace")]
    [InlineData("add")]
    [InlineData("clear")]
    public async Task UpdateAsync_Should_SynchronizeRoles_When_RoleAssignmentsVary(string mode)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        await Fixture.SeedRolesAsync(cancellationToken);
        var roles = await dbContext.Set<ApplicationRole>().ToDictionaryAsync(
            role => role.Name!, role => role.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);
        var userId = Guid.CreateVersion7();
        var created = await repository.AddAsync(
            CreateUser(userId, [roles["Developer"]]), "StrongPassword1!", cancellationToken);
        created.IsSuccess.ShouldBeTrue();
        var targetRoles = mode switch
        {
            "same" => new[] { roles["Developer"] },
            "replace" => [roles["Moderator"]],
            "add" => [roles["Developer"], roles["Player"]],
            _ => []
        };

        var result = await repository.UpdateAsync(
            user: CreateUser(userId, targetRoles),
            currentAccessToken: null,
            cancellationToken: cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        var user = await dbContext.Set<ApplicationUser>()
            .Include(item => item.Roles)
            .AsNoTracking()
            .SingleAsync(item => item.Id == userId, cancellationToken);
        user.Roles.Select(role => role.RoleId).ShouldBe(targetRoles, ignoreOrder: true);
    }

    [Fact(DisplayName = "AddAsync should reject unknown roles before writes when role does not exist")]
    public async Task AddAsync_Should_RejectRolesBeforeWrites_When_RoleDoesNotExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var id = Guid.CreateVersion7();
        var invalidUser = CreateUser(id, [Guid.CreateVersion7()]);

        var result = await repository.AddAsync(invalidUser, Password, cancellationToken);

        result.IsSuccess.ShouldBeFalse();
        (await dbContext.Set<ApplicationUser>().CountAsync(cancellationToken)).ShouldBe(0);
        (await dbContext.Set<ApplicationUserRole>().AnyAsync(cancellationToken)).ShouldBeFalse();
    }

    [Fact(DisplayName = "UpdateAsync should reject unknown roles before writes when role does not exist")]
    public async Task UpdateAsync_Should_RejectRolesBeforeWrites_When_RoleDoesNotExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var id = Guid.CreateVersion7();
        var created = await repository.AddAsync(CreateUser(id, []), Password, cancellationToken);
        created.IsSuccess.ShouldBeTrue();
        var invalidUser = CreateUser(id, [Guid.CreateVersion7()]);

        var result = await repository.UpdateAsync(
            user: invalidUser,
            currentAccessToken: null,
            cancellationToken: cancellationToken);

        result.IsSuccess.ShouldBeFalse();
        (await dbContext.Set<ApplicationUser>().CountAsync(cancellationToken)).ShouldBe(1);
        (await dbContext.Set<ApplicationUserRole>().AnyAsync(cancellationToken)).ShouldBeFalse();
    }

    [Theory(DisplayName = "User updates should preserve state when identity validation fails")]
    [InlineData(false, "roles")]
    [InlineData(true, "roles")]
    [InlineData(false, "claims")]
    [InlineData(true, "claims")]
    [InlineData(false, "block")]
    [InlineData(true, "block")]
    [InlineData(false, "confirmation")]
    [InlineData(true, "confirmation")]
    [InlineData(false, "email")]
    [InlineData(true, "email")]
    public async Task UpdateAsync_Should_PreserveState_When_IdentityValidationFails(bool duplicateEmail, string change)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await Fixture.SeedRolesAsync(cancellationToken);
        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var roles = await dbContext.Set<ApplicationRole>().ToDictionaryAsync(
            role => role.Name!, role => role.Id, cancellationToken);
        var id = Guid.CreateVersion7();
        var created = await repository.AddAsync(
            CreateUser(id, [roles["Player"]]), "StrongPassword1!", cancellationToken);
        created.IsSuccess.ShouldBeTrue();
        var other = IntegrationTestData.CreateUser(
            id: Guid.CreateVersion7(),
            email: "occupied@example.com",
            isConfirmed: true,
            roleIds: [],
            claims: [],
            userName: "occupied",
            status: UserStatus.Active);
        (await repository.AddAsync(other, "StrongPassword1!", cancellationToken)).IsSuccess.ShouldBeTrue();
        var authorizationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictAuthorizationManager>();
        await authorizationManager.CreateAsync(
            new OpenIddictAuthorizationDescriptor
            {
                Subject = id.ToString(),
                Type = OpenIddictConstants.AuthorizationTypes.AdHoc,
                Status = OpenIddictConstants.Statuses.Valid
            },
            cancellationToken);
        var original = await dbContext.Set<ApplicationUser>()
            .AsNoTracking()
            .SingleAsync(user => user.Id == id, cancellationToken);
        var email = duplicateEmail ? "occupied@example.com"
            : change == "email" ? "changed@example.com" : "hero@example.com";
        var updated = IntegrationTestData.CreateUser(
            id: id,
            email: email,
            isConfirmed: change == "email",
            roleIds: [roles[change == "roles" ? "Developer" : "Player"]],
            claims: change is "confirmation" or "email" ? [] : [("permission", "identity.users.read")],
            userName: duplicateEmail ? "hero" : "occupied",
            status: change == "block" ? UserStatus.Blocked : UserStatus.Active);

        var result = await repository.UpdateAsync(
            user: updated,
            currentAccessToken: null,
            cancellationToken: cancellationToken);

        result.Errors.ShouldContain(error => error.Type == ErrorType.Conflict);
        var persisted = await dbContext.Set<ApplicationUser>()
            .Include(user => user.Roles)
            .Include(user => user.Claims)
            .AsNoTracking()
            .SingleAsync(user => user.Id == id, cancellationToken);
        persisted.Email.ShouldBe(original.Email);
        persisted.UserName.ShouldBe(original.UserName);
        persisted.EmailConfirmed.ShouldBe(original.EmailConfirmed);
        persisted.Status.ShouldBe(original.Status);
        persisted.SecurityStamp.ShouldBe(original.SecurityStamp);
        persisted.Roles.ShouldHaveSingleItem().RoleId.ShouldBe(roles["Player"]);
        persisted.Claims.ShouldBeEmpty();
        var authorization = await dbContext.Set<OpenIddictEntityFrameworkCoreAuthorization<Guid>>()
            .AsNoTracking()
            .SingleAsync(item => item.Subject == id.ToString(), cancellationToken);
        authorization.Status.ShouldBe(OpenIddictConstants.Statuses.Valid);
    }

    [Theory(DisplayName = "User updates should invalidate changed sessions when session inputs vary")]
    [InlineData("same")]
    [InlineData("reorder")]
    [InlineData("add")]
    [InlineData("remove")]
    [InlineData("type")]
    [InlineData("value")]
    [InlineData("unconfirm")]
    [InlineData("email")]
    [InlineData("same-email")]
    [InlineData("name")]
    public async Task UpdateAsync_Should_InvalidateChangedSessions_When_SessionInputsVary(string mode)
    {
        var ct = TestContext.Current.CancellationToken;
        var id = await AddUserAsync(
            "hero@example.com",
            "hero",
            true,
            [new("permission", "identity.users.read"), new("permission", "identity.roles.read")],
            UserStatus.Active.Name,
            ct);
        var otherId = await AddUserAsync("other@example.com", "other", true, [], UserStatus.Active.Name, ct);
        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var authorizationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictAuthorizationManager>();
        var tokenManager = scope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();
        foreach (var subject in new[] { id, otherId })
        {
            var authorization = await authorizationManager.CreateAsync(
                new OpenIddictAuthorizationDescriptor
                {
                    Subject = subject.ToString(),
                    Type = OpenIddictConstants.AuthorizationTypes.AdHoc,
                    Status = OpenIddictConstants.Statuses.Valid
                },
                ct);
            foreach (var type in new[]
                     {
                         OpenIddictConstants.TokenTypeHints.AccessToken,
                         OpenIddictConstants.TokenTypeHints.RefreshToken
                     })
            {
                await tokenManager.CreateAsync(
                    new OpenIddictTokenDescriptor
                    {
                        Subject = subject.ToString(),
                        AuthorizationId = await authorizationManager.GetIdAsync(authorization, ct),
                        Type = type,
                        Status = OpenIddictConstants.Statuses.Valid
                    },
                    ct);
            }
        }

        var stamps = await dbContext.Set<ApplicationUser>().AsNoTracking()
            .ToDictionaryAsync(user => user.Id, user => user.SecurityStamp, ct);
        (string Type, string Value)[] claims = mode switch
        {
            "reorder" => [("permission", "identity.roles.read"), ("permission", "identity.users.read")],
            "add" => [("permission", "identity.users.read"), ("permission", "identity.roles.read"), ("team", "heroes")],
            "remove" => [("permission", "identity.users.read")],
            "type" => [("permission", "identity.users.read"), ("custom", "identity.roles.read")],
            "value" => [("permission", "identity.users.read"), ("permission", "identity.roles.update")],
            _ => [("permission", "identity.users.read"), ("permission", "identity.roles.read")]
        };
        var email = mode switch
        {
            "email" => "changed@example.com",
            "same-email" => " HERO@EXAMPLE.COM ",
            _ => "hero@example.com"
        };
        var updated = IntegrationTestData.CreateUser(
            id: id,
            email: email,
            isConfirmed: mode != "unconfirm",
            roleIds: [],
            claims: claims,
            userName: mode == "name" ? "renamed" : "hero",
            status: UserStatus.Active);

        var result = await repository.UpdateAsync(
            user: updated,
            currentAccessToken: null,
            cancellationToken: ct);

        result.IsSuccess.ShouldBeTrue();
        var changed = mode is not ("same" or "reorder" or "same-email");
        var users = await dbContext.Set<ApplicationUser>().Include(user => user.Claims).AsNoTracking()
            .ToDictionaryAsync(user => user.Id, ct);
        (users[id].SecurityStamp != stamps[id]).ShouldBe(changed);
        users[id].Email.ShouldBe(mode == "email" ? "changed@example.com" : "hero@example.com");
        users[id].EmailConfirmed.ShouldBe(mode != "unconfirm");
        users[otherId].SecurityStamp.ShouldBe(stamps[otherId]);
        users[id].Claims.Select(claim => (claim.ClaimType!, claim.ClaimValue!)).ShouldBe(claims, ignoreOrder: true);
        var authorizations = await dbContext.Set<OpenIddictEntityFrameworkCoreAuthorization<Guid>>()
            .AsNoTracking()
            .ToListAsync(ct);
        var tokens = await dbContext.Set<OpenIddictEntityFrameworkCoreToken<Guid>>()
            .AsNoTracking()
            .ToListAsync(ct);
        var expected = changed ? OpenIddictConstants.Statuses.Revoked : OpenIddictConstants.Statuses.Valid;
        authorizations.Single(item => item.Subject == id.ToString()).Status.ShouldBe(
            mode is "unconfirm" or "email" ? OpenIddictConstants.Statuses.Revoked : OpenIddictConstants.Statuses.Valid);
        foreach (var token in tokens.Where(item => item.Subject == id.ToString()))
        {
            token.Status.ShouldBe(mode is "unconfirm" or "email"
                || token.Type == OpenIddictConstants.TokenTypeHints.AccessToken
                ? expected
                : OpenIddictConstants.Statuses.Valid);
        }
        authorizations.Single(item => item.Subject == otherId.ToString()).Status
            .ShouldBe(OpenIddictConstants.Statuses.Valid);
        tokens.Where(item => item.Subject == otherId.ToString())
            .ShouldAllBe(item => item.Status == OpenIddictConstants.Statuses.Valid);
    }

    private static User CreateUser(Guid id, IReadOnlyCollection<Guid> roleIds) =>
        IntegrationTestData.CreateUser(
            id: id,
            email: "hero@example.com",
            isConfirmed: true,
            roleIds: roleIds,
            claims: [],
            userName: "hero",
            status: UserStatus.Active);

    private async Task<Guid> AddUserAsync(
        string email,
        string userName,
        bool isConfirmed,
        IReadOnlyCollection<Claim> claims,
        string status,
        CancellationToken cancellationToken)
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var result = await repository.AddAsync(
            IntegrationTestData.CreateUser(
                id: Guid.CreateVersion7(),
                email: email,
                isConfirmed: isConfirmed,
                roleIds: [],
                claims: claims.Select(claim => (claim.Type, claim.Value)),
                userName: userName,
                status: UserStatus.Create(value: status).Value),
            Password,
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();

        return result.Value;
    }

    private async Task<string> ReadStatusAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();

        return await dbContext.Set<ApplicationUser>()
            .Where(item => item.Id == userId)
            .Select(item => item.Status)
            .SingleAsync(cancellationToken);
    }
}
