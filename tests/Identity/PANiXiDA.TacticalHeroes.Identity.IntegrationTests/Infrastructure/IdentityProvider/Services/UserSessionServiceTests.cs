using System.Data.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Services;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.IdentityProvider.Services;

public sealed class UserSessionServiceTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Theory(DisplayName = "Bulk revocation should use three updates when session counts and preservation vary")]
    [InlineData(1, false)]
    [InlineData(8, false)]
    [InlineData(1, true)]
    [InlineData(8, true)]
    public async Task RevokeManyAsync_Should_UseThreeUpdates_When_SessionCountsAndPreservationVary(
        int count, bool preserve)
    {
        var ct = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var users = Enumerable.Range(0, 3)
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
            userIds: [users[0].Id, users[1].Id],
            preservedAuthorizationId: preserve ? currentId : null,
            cancellationToken: ct);

        counter.Count.ShouldBe(3);
        var persistedUsers = await bulkContext.Users.AsNoTracking().ToDictionaryAsync(user => user.Id, ct);
        persistedUsers[users[0].Id].SecurityStamp.ShouldNotBe(originalStamps[users[0].Id]);
        persistedUsers[users[1].Id].SecurityStamp.ShouldNotBe(originalStamps[users[1].Id]);
        persistedUsers[users[0].Id].SecurityStamp.ShouldNotBe(persistedUsers[users[1].Id].SecurityStamp);
        persistedUsers[users[2].Id].SecurityStamp.ShouldBe(originalStamps[users[2].Id]);
        var persistedAuthorizations = await bulkContext.Set<OpenIddictEntityFrameworkCoreAuthorization<Guid>>()
            .AsNoTracking()
            .ToListAsync(ct);
        foreach (var authorization in persistedAuthorizations)
        {
            var valid = authorization.Subject == users[2].Id.ToString()
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
            var valid = token.Subject == users[2].Id.ToString()
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

        public override ValueTask<int> NonQueryExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result,
            CancellationToken cancellationToken)
        {
            if (command.CommandText.StartsWith("UPDATE", StringComparison.Ordinal))
            {
                Count++;
            }

            return ValueTask.FromResult(result);
        }
    }
}
