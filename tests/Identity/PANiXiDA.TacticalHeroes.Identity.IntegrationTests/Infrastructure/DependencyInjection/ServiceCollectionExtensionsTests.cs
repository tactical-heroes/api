using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.DependencyInjection;

public sealed class ServiceCollectionExtensionsTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "AddInfrastructure should resolve read repositories")]
    public async Task AddInfrastructure_Should_ResolveReadRepositories()
    {
        await using var scope = Fixture.CreateScope();

        var usersReadRepository = scope.ServiceProvider.GetRequiredService<IUsersReadRepository>();
        var rolesReadRepository = scope.ServiceProvider.GetRequiredService<IRolesReadRepository>();

        usersReadRepository.ShouldNotBeNull();
        rolesReadRepository.ShouldNotBeNull();
    }
}
