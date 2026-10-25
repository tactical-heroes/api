using System.Data.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Services;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.IdentityProvider.Services;

public sealed class UserSessionServiceTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Theory(DisplayName = "Session lookup should enforce ownership and validity when current token state varies")]
    [InlineData("valid")]
    [InlineData("foreign-token")]
    [InlineData("foreign-authorization")]
    [InlineData("revoked-token")]
    [InlineData("revoked-authorization")]
    [InlineData("wrong-type")]
    [InlineData("missing-token")]
    [InlineData("no-token")]
    [InlineData("empty-token")]
    public async Task GetCurrentAuthorizationIdAsync_Should_EnforceOwnershipAndValidity_When_CurrentTokenStateVaries(
        string mode)
    {
        var ct = TestContext.Current.CancellationToken;
        var userId = Guid.CreateVersion7();
        var foreignId = Guid.CreateVersion7();
        var referenceId = Guid.CreateVersion7().ToString("N");
        await using var scope = Fixture.CreateScope();
        var authorizations = scope.ServiceProvider.GetRequiredService<IOpenIddictAuthorizationManager>();
        var authorization = await authorizations.CreateAsync(
            new OpenIddictAuthorizationDescriptor
            {
                Subject = (mode == "foreign-authorization" ? foreignId : userId).ToString(),
                Status = mode == "revoked-authorization"
                    ? OpenIddictConstants.Statuses.Revoked
                    : OpenIddictConstants.Statuses.Valid,
                Type = OpenIddictConstants.AuthorizationTypes.AdHoc
            },
            ct);
        var authorizationId = await authorizations.GetIdAsync(authorization, ct);
        authorizationId.ShouldNotBeNull();
        var tokens = scope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();
        await tokens.CreateAsync(
            new OpenIddictTokenDescriptor
            {
                Subject = (mode == "foreign-token" ? foreignId : userId).ToString(),
                AuthorizationId = authorizationId,
                ReferenceId = referenceId,
                Status = mode == "revoked-token"
                    ? OpenIddictConstants.Statuses.Revoked
                    : OpenIddictConstants.Statuses.Valid,
                Type = mode == "wrong-type"
                    ? OpenIddictConstants.TokenTypeIdentifiers.RefreshToken
                    : OpenIddictConstants.TokenTypeIdentifiers.AccessToken
            },
            ct);
        var service = scope.ServiceProvider.GetRequiredService<IUserSessionService>();
        service.ShouldBeSameAs(scope.ServiceProvider.GetRequiredService<UserSessionService>());
        var token = mode switch
        {
            "no-token" => null,
            "empty-token" => "",
            "missing-token" => Guid.CreateVersion7().ToString("N"),
            _ => referenceId
        };

        var result = await service.GetCurrentAuthorizationIdAsync(userId, token, ct);

        var isAllowed = mode is "valid" or "no-token" or "empty-token";
        result.IsSuccess.ShouldBe(isAllowed);
        if (isAllowed)
        {
            result.Value.ShouldBe(mode == "valid" ? authorizationId : null);
        }
        else
        {
            result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Unauthorized);
        }
    }

    [Fact(DisplayName = "Session validation should reject a cached authorization when database status is revoked")]
    public async Task IsAuthorizationValidAsync_Should_RejectCachedAuthorization_When_DatabaseStatusIsRevoked()
    {
        var ct = TestContext.Current.CancellationToken;
        var userId = Guid.CreateVersion7();
        await using var scope = Fixture.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictAuthorizationManager>();
        var authorization = await manager.CreateAsync(
            new OpenIddictAuthorizationDescriptor
            {
                Subject = userId.ToString(),
                Status = OpenIddictConstants.Statuses.Valid,
                Type = OpenIddictConstants.AuthorizationTypes.AdHoc
            },
            ct);
        var id = await manager.GetIdAsync(authorization, ct);
        id.ShouldNotBeNull();
        var service = scope.ServiceProvider.GetRequiredService<UserSessionService>();
        (await service.IsAuthorizationValidAsync(userId, id, ct)).ShouldBeTrue();
        await using (var writer = Fixture.CreateScope())
        {
            var db = writer.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
            var value = Guid.Parse(id);
            await db.Set<OpenIddictEntityFrameworkCoreAuthorization<Guid>>()
                .Where(item => item.Id == value)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(item => item.Status, OpenIddictConstants.Statuses.Revoked),
                    ct);
        }

        var valid = await service.IsAuthorizationValidAsync(userId, id, ct);

        valid.ShouldBeFalse();
        (await manager.HasStatusAsync(authorization, OpenIddictConstants.Statuses.Valid, ct)).ShouldBeTrue();
    }

    [Theory(DisplayName = "Session validation should reject invalid identifiers when authorization does not match")]
    [InlineData("foreign")]
    [InlineData("missing")]
    [InlineData("malformed")]
    public async Task IsAuthorizationValidAsync_Should_RejectInvalidIdentifiers_When_AuthorizationDoesNotMatch(
        string mode)
    {
        var ct = TestContext.Current.CancellationToken;
        var userId = Guid.CreateVersion7();
        await using var scope = Fixture.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictAuthorizationManager>();
        var authorization = await manager.CreateAsync(
            new OpenIddictAuthorizationDescriptor
            {
                Subject = Guid.CreateVersion7().ToString(),
                Status = OpenIddictConstants.Statuses.Valid,
                Type = OpenIddictConstants.AuthorizationTypes.AdHoc
            },
            ct);
        var id = await manager.GetIdAsync(authorization, ct);
        id.ShouldNotBeNull();
        var candidate = mode switch
        {
            "missing" => Guid.CreateVersion7().ToString(),
            "malformed" => "not-an-authorization",
            _ => id
        };
        var service = scope.ServiceProvider.GetRequiredService<UserSessionService>();

        var valid = await service.IsAuthorizationValidAsync(userId, candidate, ct);

        valid.ShouldBeFalse();
    }

    [Theory(
        DisplayName = "Session revocation should affect one grant when ownership and transaction vary")]
    [InlineData("valid")]
    [InlineData("rollback")]
    [InlineData("foreign-user")]
    [InlineData("foreign-client")]
    [InlineData("missing")]
    [InlineData("malformed")]
    [InlineData("revoked")]
    public async Task RevokeSessionAsync_Should_AffectOneGrant_When_OwnershipAndTransactionVary(string mode)
    {
        var ct = TestContext.Current.CancellationToken;
        var userId = Guid.CreateVersion7();
        var foreignId = Guid.CreateVersion7();
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var applications = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var application = await applications.CreateAsync(
            new OpenIddictApplicationDescriptor { ClientId = "logout-test" },
            ct);
        var applicationId = await applications.GetIdAsync(application, ct);
        var authorizations = scope.ServiceProvider.GetRequiredService<IOpenIddictAuthorizationManager>();
        var tokens = scope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();
        var ids = new List<string>();
        foreach (var subject in new[] { userId, userId, foreignId })
        {
            var authorization = await authorizations.CreateAsync(
                new OpenIddictAuthorizationDescriptor
                {
                    Subject = subject.ToString(),
                    ApplicationId = applicationId,
                    Type = OpenIddictConstants.AuthorizationTypes.AdHoc,
                    Status = OpenIddictConstants.Statuses.Valid
                },
                ct);
            var id = await authorizations.GetIdAsync(authorization, ct);
            id.ShouldNotBeNull();
            ids.Add(id);
            foreach (var type in new[]
                     {
                         OpenIddictConstants.TokenTypeIdentifiers.AccessToken,
                         OpenIddictConstants.TokenTypeIdentifiers.RefreshToken,
                         OpenIddictConstants.TokenTypeIdentifiers.IdentityToken,
                         OpenIddictConstants.TokenTypeIdentifiers.Private.AuthorizationCode
                     })
            {
                await tokens.CreateAsync(
                    new OpenIddictTokenDescriptor
                    {
                        Subject = subject.ToString(),
                        ApplicationId = applicationId,
                        AuthorizationId = id,
                        Type = type,
                        Status = OpenIddictConstants.Statuses.Valid
                    },
                    ct);
            }
        }
        if (mode == "revoked")
        {
            var authorization = await authorizations.FindByIdAsync(ids[0], ct);
            (await authorizations.TryRevokeAsync(authorization!, ct)).ShouldBeTrue();
        }
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var service = scope.ServiceProvider.GetRequiredService<IUserSessionService>();
        service.ShouldBeSameAs(scope.ServiceProvider.GetRequiredService<UserSessionService>());
        var selected = mode switch
        {
            "missing" => Guid.CreateVersion7().ToString(),
            "malformed" => "invalid",
            _ => ids[0]
        };

        var result = await service.RevokeSessionAsync(
            mode == "foreign-user" ? foreignId : userId,
            selected,
            mode == "foreign-client" ? "other-client" : "logout-test",
            ct);
        if (mode == "rollback")
        {
            await transaction.RollbackAsync(ct);
        }
        else
        {
            await transaction.CommitAsync(ct);
        }

        var success = mode is "valid" or "rollback" or "revoked";
        result.IsSuccess.ShouldBe(success);
        var stored = await db.Set<OpenIddictEntityFrameworkCoreAuthorization<Guid>>().AsNoTracking().ToArrayAsync(ct);
        foreach (var authorization in stored)
        {
            var revoked = authorization.Id.ToString() == ids[0] && mode is "valid" or "revoked";
            authorization.Status.ShouldBe(revoked
                ? OpenIddictConstants.Statuses.Revoked
                : OpenIddictConstants.Statuses.Valid);
        }
        var storedTokens = await db.Set<OpenIddictEntityFrameworkCoreToken<Guid>>()
            .Include(token => token.Authorization)
            .AsNoTracking()
            .ToArrayAsync(ct);
        foreach (var token in storedTokens)
        {
            var revoked = token.Authorization!.Id.ToString() == ids[0] && mode is "valid" or "revoked";
            token.Status.ShouldBe(revoked
                ? OpenIddictConstants.Statuses.Revoked
                : OpenIddictConstants.Statuses.Valid);
        }
    }

    [Theory(DisplayName = "Bulk access invalidation should use two updates when membership varies")]
    [InlineData(2, 1, false)]
    [InlineData(2, 8, false)]
    [InlineData(2, 1, true)]
    [InlineData(2, 8, true)]
    [InlineData(64, 1, false)]
    [InlineData(64, 1, true)]
    public async Task InvalidateRoleMemberAccessTokensAsync_Should_UseTwoUpdates_When_MembershipVaries(
        int memberCount, int count, bool preserve)
    {
        var ct = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var users = Enumerable.Range(0, memberCount + 1)
            .Select(index => new ApplicationUser
            {
                Id = Guid.CreateVersion7(),
                UserName = $"member-{index}",
                NormalizedUserName = $"MEMBER-{index}",
                Email = $"member-{index}@example.test",
                NormalizedEmail = $"MEMBER-{index}@EXAMPLE.TEST",
                Status = "Active",
                PasswordHash = "unused-test-password-hash",
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            })
            .ToArray();
        dbContext.AddRange(users);
        var role = new ApplicationRole
        {
            Id = Guid.CreateVersion7(),
            Name = "members",
            NormalizedName = "MEMBERS"
        };
        dbContext.Add(role);
        dbContext.AddRange(users.Take(memberCount).Select(user => new ApplicationUserRole
        {
            UserId = user.Id,
            RoleId = role.Id
        }));
        await dbContext.SaveChangesAsync(ct);
        var authorizations = scope.ServiceProvider.GetRequiredService<IOpenIddictAuthorizationManager>();
        var tokens = scope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();
        var currentId = "";
        foreach (var user in users)
        {
            for (var index = 0; index < count; index++)
            {
                var authorization = await authorizations.CreateAsync(
                    new OpenIddictAuthorizationDescriptor
                    {
                        Subject = user.Id.ToString(),
                        Status = OpenIddictConstants.Statuses.Valid,
                        Type = OpenIddictConstants.AuthorizationTypes.AdHoc
                    },
                    ct);
                var id = await authorizations.GetIdAsync(authorization, ct);
                if (user == users[0] && index == 0)
                {
                    currentId = id!;
                }
                foreach (var type in new[]
                         {
                             OpenIddictConstants.TokenTypeIdentifiers.AccessToken,
                             OpenIddictConstants.TokenTypeIdentifiers.RefreshToken,
                             OpenIddictConstants.TokenTypeHints.RefreshToken,
                             OpenIddictConstants.TokenTypeIdentifiers.Private.AuthorizationCode
                         })
                {
                    await tokens.CreateAsync(
                        new OpenIddictTokenDescriptor
                        {
                            Subject = user.Id.ToString(),
                            AuthorizationId = id,
                            Type = type,
                            Status = OpenIddictConstants.Statuses.Valid
                        },
                        ct);
                }
            }
        }
        var orphan = await tokens.CreateAsync(
            new OpenIddictTokenDescriptor
            {
                Subject = users[0].Id.ToString(),
                Type = OpenIddictConstants.TokenTypeIdentifiers.RefreshToken,
                Status = OpenIddictConstants.Statuses.Valid
            },
            ct);
        var originalStamps = users.ToDictionary(user => user.Id, user => user.SecurityStamp);
        var counter = new BulkUpdateCounter();
        var options = new DbContextOptionsBuilder<IdentityWriteDbContext>(
            scope.ServiceProvider.GetRequiredService<DbContextOptions<IdentityWriteDbContext>>())
            .AddInterceptors(counter)
            .Options;
        await using var bulkContext = new IdentityWriteDbContext(options, []);
        var service = new UserSessionService(tokens, authorizations, bulkContext);

        await service.InvalidateRoleMemberAccessTokensAsync(
            roleId: role.Id,
            refreshCurrentSession: preserve,
            cancellationToken: ct);

        counter.Count.ShouldBe(2);
        counter.ReadCount.ShouldBe(0);
        counter.SubqueryCount.ShouldBe(2);
        var persistedUsers = await bulkContext.Users.AsNoTracking().ToDictionaryAsync(user => user.Id, ct);
        foreach (var user in users.Take(memberCount))
        {
            persistedUsers[user.Id].SecurityStamp.ShouldNotBe(originalStamps[user.Id]);
        }
        persistedUsers[users[0].Id].SecurityStamp.ShouldNotBe(persistedUsers[users[1].Id].SecurityStamp);
        persistedUsers[users[^1].Id].SecurityStamp.ShouldBe(originalStamps[users[^1].Id]);
        var persistedAuthorizations = await bulkContext.Set<OpenIddictEntityFrameworkCoreAuthorization<Guid>>()
            .AsNoTracking()
            .ToListAsync(ct);
        foreach (var authorization in persistedAuthorizations)
        {
            authorization.Status.ShouldBe(OpenIddictConstants.Statuses.Valid);
        }
        var persistedTokens = await bulkContext.Set<OpenIddictEntityFrameworkCoreToken<Guid>>()
            .Include(token => token.Authorization)
            .AsNoTracking()
            .ToListAsync(ct);
        foreach (var token in persistedTokens)
        {
            var valid = token.Subject == users[^1].Id.ToString()
                || token.Type is not (OpenIddictConstants.TokenTypeIdentifiers.AccessToken
                    or OpenIddictConstants.TokenTypeHints.AccessToken);
            token.Status.ShouldBe(valid ? OpenIddictConstants.Statuses.Valid : OpenIddictConstants.Statuses.Revoked);
        }
        var currentTokens = persistedTokens.Where(token => token.Authorization?.Id.ToString() == currentId);
        currentTokens.Count(token => token.Status == OpenIddictConstants.Statuses.Valid).ShouldBe(3);
        (await tokens.TryRedeemAsync(orphan, ct)).ShouldBeTrue();
    }

    private sealed class BulkUpdateCounter : DbCommandInterceptor
    {
        internal int Count { get; private set; }
        internal int ReadCount { get; private set; }
        internal int SubqueryCount { get; private set; }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken)
        {
            ReadCount++;

            return ValueTask.FromResult(result);
        }

        public override ValueTask<int> NonQueryExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result,
            CancellationToken cancellationToken)
        {
            if (command.CommandText.StartsWith("UPDATE", StringComparison.Ordinal))
            {
                Count++;
                if (command.CommandText.Contains("asp_net_user_roles", StringComparison.Ordinal))
                {
                    SubqueryCount++;
                }
            }

            return ValueTask.FromResult(result);
        }
    }
}
