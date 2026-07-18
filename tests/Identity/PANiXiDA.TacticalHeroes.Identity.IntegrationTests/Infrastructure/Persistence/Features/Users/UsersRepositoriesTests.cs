using System.Security.Claims;

using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Features.Users;

public sealed class UsersRepositoriesTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    private const string Password = "StrongPassword1!";

    [Fact(DisplayName = "Users repositories should persist and read user state")]
    public async Task UsersRepositories_Should_PersistAndReadUserState()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        Guid userId;

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUsersWriteRepository>();
            var result = await repository.AddAsync(
                "hero@example.com",
                "hero",
                Password,
                true,
                [new Claim(type: "permission", value: "identity.profile.read")],
                UserStatus.Active.Name,
                cancellationToken);

            result.IsSuccess.ShouldBeTrue();
            userId = result.Value;
        }

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUsersWriteRepository>();
            var result = await repository.UpdateAsync(
                userId,
                "renamed@example.com",
                "renamed",
                true,
                [new Claim(type: "permission", value: "identity.users.manage")],
                UserStatus.Blocked.Name,
                cancellationToken);

            result.IsSuccess.ShouldBeTrue();
        }

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUsersReadRepository>();
            (await repository.ExistsByIdAsync(
                id: userId,
                cancellationToken: cancellationToken)).ShouldBeTrue();
            var pageResult = await repository.GetPagedAsync(
                email: "renamed@example.com",
                pagination: new PaginationParameters(PageNumber: 1, PageSize: 10),
                cancellationToken: cancellationToken);
            var userDetails = await repository.GetDetailsByIdAsync(
                id: userId,
                cancellationToken: cancellationToken);

            var user = pageResult.Items.ShouldHaveSingleItem();
            user.StatusDisplayName.ShouldBe(UserStatus.Blocked.DisplayName);
            userDetails.ShouldNotBeNull();
            userDetails.Email.ShouldBe("renamed@example.com");
            userDetails.UserName.ShouldBe("renamed");
            userDetails.Status.ShouldBe(UserStatus.Blocked.Name);
            userDetails.StatusDisplayName.ShouldBe(UserStatus.Blocked.DisplayName);
            var claim = userDetails.Claims.ShouldHaveSingleItem();
            claim.Type.ShouldBe("permission");
            claim.Value.ShouldBe("identity.users.manage");
        }

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUsersWriteRepository>();
            (await repository.DeleteAsync(userId, cancellationToken)).IsSuccess.ShouldBeTrue();
        }

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUsersReadRepository>();
            (await repository.GetDetailsByIdAsync(
                id: userId,
                cancellationToken: cancellationToken)).ShouldBeNull();
        }
    }
}
