using System.Security.Claims;

using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;
namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Features.Roles;

public sealed class RolesRepositoriesTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Roles repositories should persist and read role state")]
    public async Task RolesRepositories_Should_PersistAndReadRoleState()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        Guid roleId;

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRolesWriteRepository>();
            var result = await repository.AddAsync(
                "operator",
                [new Claim(type: "permission", value: "identity.profile.read")],
                cancellationToken);

            result.IsSuccess.ShouldBeTrue();
            roleId = result.Value;
        }

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRolesWriteRepository>();
            var result = await repository.UpdateAsync(
                roleId,
                "administrator",
                [new Claim(type: "permission", value: "identity.accounts.manage")],
                cancellationToken);

            result.IsSuccess.ShouldBeTrue();
        }

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRolesReadRepository>();
            var result = await repository.GetDetailsByIdAsync(roleId, cancellationToken);

            result.IsSuccess.ShouldBeTrue();
            result.Value.Name.ShouldBe("administrator");
            var claim = result.Value.Claims.ShouldHaveSingleItem();
            claim.Type.ShouldBe("permission");
            claim.Value.ShouldBe("identity.accounts.manage");
        }

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRolesWriteRepository>();
            (await repository.DeleteAsync(roleId, cancellationToken)).IsSuccess.ShouldBeTrue();
        }

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRolesReadRepository>();
            (await repository.GetDetailsByIdAsync(roleId, cancellationToken)).IsFailure.ShouldBeTrue();
        }
    }
}
