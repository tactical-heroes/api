using System.Security.Claims;

using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Features.Accounts;

public sealed class AccountsRepositoriesTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    private const string Password = "StrongPassword1!";

    [Fact(DisplayName = "Accounts repositories should persist and read account state")]
    public async Task AccountsRepositories_Should_PersistAndReadAccountState()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        Guid accountId;

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IAccountsWriteRepository>();
            var result = await repository.AddAsync(
                "hero@example.com",
                "hero",
                Password,
                true,
                [new Claim("permission", "identity.profile.read")],
                UserStatus.Active.Name,
                cancellationToken);

            result.IsSuccess.ShouldBeTrue();
            accountId = result.Value;
        }

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IAccountsWriteRepository>();
            var result = await repository.UpdateAsync(
                accountId,
                "renamed@example.com",
                "renamed",
                true,
                [new Claim("permission", "identity.accounts.manage")],
                UserStatus.Blocked.Name,
                cancellationToken);

            result.IsSuccess.ShouldBeTrue();
        }

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IAccountsReadRepository>();
            var result = await repository.GetDetailsByIdAsync(accountId, cancellationToken);

            result.IsSuccess.ShouldBeTrue();
            result.Value.Email.ShouldBe("renamed@example.com");
            result.Value.UserName.ShouldBe("renamed");
            result.Value.Status.ShouldBe(UserStatus.Blocked.Name);
            result.Value.StatusDisplayName.ShouldBe(UserStatus.Blocked.DisplayName);
            var claim = result.Value.Claims.ShouldHaveSingleItem();
            claim.Type.ShouldBe("permission");
            claim.Value.ShouldBe("identity.accounts.manage");
        }

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IAccountsWriteRepository>();
            (await repository.DeleteAsync(accountId, cancellationToken)).IsSuccess.ShouldBeTrue();
        }

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IAccountsReadRepository>();
            (await repository.GetDetailsByIdAsync(accountId, cancellationToken)).IsFailure.ShouldBeTrue();
        }
    }
}
