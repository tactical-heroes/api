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
            var pageResult = await repository.GetPagedAsync(
                email: "renamed@example.com",
                pagination: new PaginationParameters(PageNumber: 1, PageSize: 10),
                cancellationToken: cancellationToken);
            var result = await repository.GetDetailsByIdAsync(userId, cancellationToken);

            pageResult.IsSuccess.ShouldBeTrue();
            var user = pageResult.Value.Items.ShouldHaveSingleItem();
            user.StatusDisplayName.ShouldBe(UserStatus.Blocked.DisplayName);
            result.IsSuccess.ShouldBeTrue();
            result.Value.Email.ShouldBe("renamed@example.com");
            result.Value.UserName.ShouldBe("renamed");
            result.Value.Status.ShouldBe(UserStatus.Blocked.Name);
            result.Value.StatusDisplayName.ShouldBe(UserStatus.Blocked.DisplayName);
            var claim = result.Value.Claims.ShouldHaveSingleItem();
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
            (await repository.GetDetailsByIdAsync(userId, cancellationToken)).IsFailure.ShouldBeTrue();
        }
    }
}
