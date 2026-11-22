using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation;

[Collection(FunctionalTestCollectionDefinition.Name)]
public sealed class FunctionalTestFixtureTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "Database reset should restore seeded roles and cached OAuth clients when seed data changes")]
    public async Task ResetDatabase_Should_RestoreSeedData_When_SeedDataChanges()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using (var scope = Fixture.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
            await dbContext.Set<ApplicationRole>()
                .Where(role => role.NormalizedName == "PLAYER")
                .ExecuteUpdateAsync(updates => updates.SetProperty(role => role.Name, "Changed"), cancellationToken);
            var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
            var application = (OpenIddictEntityFrameworkCoreApplication<Guid>)(await manager.FindByClientIdAsync(
                "tactical-heroes-service", cancellationToken))!;
            application.ClientSecret = "changed";
            application.Permissions = "[]";
            await manager.UpdateAsync(application, cancellationToken);
        }

        await Fixture.ResetDatabaseAsync(cancellationToken);
        var token = await OAuthServiceAccessTokenTestHelper.IssueAccessTokenAsync(Fixture, null, cancellationToken);

        token.ShouldNotBeNullOrWhiteSpace();
        await using var verificationScope = Fixture.Services.CreateAsyncScope();
        var restored = verificationScope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var role = await restored.Set<ApplicationRole>()
            .SingleAsync(role => role.NormalizedName == "PLAYER", cancellationToken);
        role.Name.ShouldBe("Player");
    }
}
