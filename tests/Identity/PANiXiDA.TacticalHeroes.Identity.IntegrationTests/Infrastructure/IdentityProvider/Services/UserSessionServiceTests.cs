using System.Data.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Services;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.IdentityProvider.Services;

public sealed class UserSessionServiceTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Theory(DisplayName = "Bulk revocation should use three updates when member counts sessions and preservation vary")]
    [InlineData(2, 1, false)]
    [InlineData(2, 8, false)]
    [InlineData(2, 1, true)]
    [InlineData(2, 8, true)]
    [InlineData(64, 1, false)]
    [InlineData(64, 1, true)]
    public async Task RevokeManyAsync_Should_UseThreeUpdates_When_MemberCountsSessionsAndPreservationVary(
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

        await service.RevokeManyAsync(
            userIds: bulkContext.Set<ApplicationUserRole>()
                .Where(link => link.RoleId == role.Id)
                .Select(link => link.UserId),
            preservedAuthorizationId: preserve ? currentId : null,
            cancellationToken: ct);

        counter.Count.ShouldBe(3);
        counter.ReadCount.ShouldBe(0);
        counter.SubqueryCount.ShouldBe(3);
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
            var valid = authorization.Subject == users[^1].Id.ToString()
                || (preserve && authorization.Id.ToString() == currentId);
            authorization.Status.ShouldBe(valid
                ? OpenIddictConstants.Statuses.Valid
                : OpenIddictConstants.Statuses.Revoked);
        }
        var persistedTokens = await bulkContext.Set<OpenIddictEntityFrameworkCoreToken<Guid>>()
            .Include(token => token.Authorization)
            .AsNoTracking()
            .ToListAsync(ct);
        foreach (var token in persistedTokens)
        {
            var valid = token.Subject == users[^1].Id.ToString()
                || (preserve
                    && token.Authorization?.Id.ToString() == currentId
                    && token.Type is OpenIddictConstants.TokenTypeHints.RefreshToken
                        or OpenIddictConstants.TokenTypeIdentifiers.RefreshToken);
            token.Status.ShouldBe(valid ? OpenIddictConstants.Statuses.Valid : OpenIddictConstants.Statuses.Revoked);
        }
        var currentTokens = persistedTokens.Where(token => token.Authorization?.Id.ToString() == currentId);
        currentTokens.Count(token => token.Status == OpenIddictConstants.Statuses.Valid).ShouldBe(preserve ? 2 : 0);
        (await tokens.TryRedeemAsync(orphan, ct)).ShouldBeFalse();
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
