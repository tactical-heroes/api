using System.Security.Claims;

using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Features.Users.Read;

public sealed class UsersReadRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    private const string Password = "StrongPassword1!";

    [Fact(DisplayName = "Users read repository should return persisted details and a filtered page")]
    public async Task Repository_Should_ReturnDetailsAndFilteredPage_When_UserExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        Guid firstUserId;

        await using (var scope = Fixture.CreateScope())
        {
            var writeRepository = scope.ServiceProvider.GetRequiredService<IUsersWriteRepository>();
            firstUserId = (await writeRepository.AddAsync(
                "first@example.com",
                "first-hero",
                Password,
                true,
                [new Claim("permission", "heroes.read")],
                UserStatus.Active.Name,
                cancellationToken)).Value;
            (await writeRepository.AddAsync(
                "second@example.com",
                "second-hero",
                Password,
                false,
                [],
                UserStatus.Blocked.Name,
                cancellationToken)).IsSuccess.ShouldBeTrue();
        }

        await using var verificationScope = Fixture.CreateScope();
        var readRepository = verificationScope.ServiceProvider.GetRequiredService<IUsersReadRepository>();
        var details = await readRepository.GetDetailsByIdAsync(firstUserId, cancellationToken);
        var page = await readRepository.GetPagedAsync(
            "first@example.com",
            new PaginationParameters(1, 20),
            cancellationToken);

        details.ShouldNotBeNull();
        details.Email.ShouldBe("first@example.com");
        details.UserName.ShouldBe("first-hero");
        details.IsConfirmed.ShouldBeTrue();
        details.Status.ShouldBe(UserStatus.Active.Name);
        details.Claims.ShouldHaveSingleItem().Value.ShouldBe("heroes.read");
        page.TotalCount.ShouldBe(1);
        page.Items.ShouldHaveSingleItem().Id.ShouldBe(firstUserId);
        (await readRepository.ExistsByIdAsync(firstUserId, cancellationToken)).ShouldBeTrue();
    }
}
