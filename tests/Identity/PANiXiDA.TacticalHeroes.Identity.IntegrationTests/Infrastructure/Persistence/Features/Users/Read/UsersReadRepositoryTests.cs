using System.Security.Claims;

using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Features.Users.Read;

public sealed class UsersReadRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    private const string Password = "StrongPassword1!";

    [Fact(DisplayName = "GetDetailsByIdAsync should return user details when user exists")]
    public async Task GetDetailsByIdAsync_Should_ReturnDetails_When_UserExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = await AddUserAsync(
            "details@example.com",
            "details-hero",
            isConfirmed: true,
            [new Claim("permission", "heroes.read")],
            UserStatus.Active.Name,
            cancellationToken);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersReadRepository>();
        var details = await repository.GetDetailsByIdAsync(userId, cancellationToken);

        details.ShouldNotBeNull();
        details.Email.ShouldBe("details@example.com");
        details.UserName.ShouldBe("details-hero");
        details.IsConfirmed.ShouldBeTrue();
        details.Status.ShouldBe(UserStatus.Active.Name);
        details.Claims.ShouldHaveSingleItem().Value.ShouldBe("heroes.read");
    }

    [Fact(DisplayName = "GetPagedAsync should return users filtered by email when users exist")]
    public async Task GetPagedAsync_Should_ReturnFilteredPage_When_UsersExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var firstUserId = await AddUserAsync(
            "first@example.com",
            "first-hero",
            isConfirmed: true,
            [],
            UserStatus.Active.Name,
            cancellationToken);
        await AddUserAsync(
            "second@example.com",
            "second-hero",
            isConfirmed: false,
            [],
            UserStatus.Blocked.Name,
            cancellationToken);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersReadRepository>();
        var page = await repository.GetPagedAsync(
            "first@example.com",
            new PaginationParameters(1, 20),
            cancellationToken);

        page.TotalCount.ShouldBe(1);
        page.Items.ShouldHaveSingleItem().Id.ShouldBe(firstUserId);
    }

    [Fact(DisplayName = "ExistsByIdAsync should return true for an existing user when user exists")]
    public async Task ExistsByIdAsync_Should_ReturnTrue_When_UserExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = await AddUserAsync(
            "exists@example.com",
            "exists-hero",
            isConfirmed: true,
            [],
            UserStatus.Active.Name,
            cancellationToken);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersReadRepository>();

        (await repository.ExistsByIdAsync(userId, cancellationToken)).ShouldBeTrue();
    }

    [Fact(DisplayName = "AnyAsync should reflect whether users exist when called")]
    public async Task AnyAsync_Should_ReflectWhetherUsersExist_When_Called()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await using (var emptyScope = Fixture.CreateScope())
        {
            var emptyRepository =
                emptyScope.ServiceProvider.GetRequiredService<IUsersReadRepository>();
            (await emptyRepository.AnyAsync(cancellationToken)).ShouldBeFalse();
        }

        await AddUserAsync(
            "any@example.com",
            "any-hero",
            isConfirmed: true,
            [],
            UserStatus.Active.Name,
            cancellationToken);

        await using var populatedScope = Fixture.CreateScope();
        var populatedRepository =
            populatedScope.ServiceProvider.GetRequiredService<IUsersReadRepository>();
        (await populatedRepository.AnyAsync(cancellationToken)).ShouldBeTrue();
    }

    private async Task<Guid> AddUserAsync(
        string email,
        string userName,
        bool isConfirmed,
        IReadOnlyCollection<Claim> claims,
        string status,
        CancellationToken cancellationToken)
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersWriteRepository>();
        var result = await repository.AddAsync(
            email,
            userName,
            Password,
            isConfirmed,
            claims,
            status,
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();

        return result.Value;
    }
}
