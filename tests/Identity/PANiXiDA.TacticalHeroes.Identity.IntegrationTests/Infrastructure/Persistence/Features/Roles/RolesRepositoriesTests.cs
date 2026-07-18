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
                [new Claim(type: "permission", value: "identity.users.manage")],
                cancellationToken);

            result.IsSuccess.ShouldBeTrue();
        }

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRolesReadRepository>();
            (await repository.ExistsByIdAsync(
                id: roleId,
                cancellationToken: cancellationToken)).ShouldBeTrue();
            var pageResult = await repository.GetPagedAsync(
                pagination: new PaginationParameters(PageNumber: 1, PageSize: 100),
                cancellationToken: cancellationToken);
            var roleDetails = await repository.GetDetailsByIdAsync(
                id: roleId,
                cancellationToken: cancellationToken);

            pageResult.Items.ShouldContain(role =>
                role.Id == roleId && role.Name == "administrator");
            roleDetails.ShouldNotBeNull();
            roleDetails.Name.ShouldBe("administrator");
            var claim = roleDetails.Claims.ShouldHaveSingleItem();
            claim.Type.ShouldBe("permission");
            claim.Value.ShouldBe("identity.users.manage");
        }

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRolesWriteRepository>();
            (await repository.DeleteAsync(roleId, cancellationToken)).IsSuccess.ShouldBeTrue();
        }

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRolesReadRepository>();
            (await repository.GetDetailsByIdAsync(
                id: roleId,
                cancellationToken: cancellationToken)).ShouldBeNull();
        }
    }
}
