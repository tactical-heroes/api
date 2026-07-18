using Microsoft.Extensions.DependencyInjection;

using OpenIddict.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Seeding;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Features.OAuth;

public sealed class OAuthClientsRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "OAuth clients repository should read a seeded client from OpenIddict")]
    public async Task GetTokenPrincipalByClientIdAsync_Should_ReturnPrincipal_When_ClientExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var scope = Fixture.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IdentityProviderApplicationSeeder>();
        await seeder.SeedAsync(cancellationToken);
        var repository = scope.ServiceProvider.GetRequiredService<IOAuthClientsRepository>();

        var result = await repository.GetTokenPrincipalByClientIdAsync(
            "integration-tests",
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Claims.ShouldContain(claim =>
            claim.Type == OpenIddictConstants.Claims.Subject &&
            claim.Value == "integration-tests");
    }

    [Fact(DisplayName = "OAuth clients repository should return not found for a missing client")]
    public async Task GetTokenPrincipalByClientIdAsync_Should_ReturnNotFound_When_ClientDoesNotExist()
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOAuthClientsRepository>();

        var result = await repository.GetTokenPrincipalByClientIdAsync(
            "missing-client",
            TestContext.Current.CancellationToken);

        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.NotFound);
    }
}
