using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

using RoleTestData = PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Roles.IntegrationTestData;
using UserTestData = PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Users.IntegrationTestData;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence;

public sealed class IdentityPersistenceTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Theory(DisplayName = "Repositories should persist all writes when no explicit save is called")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Repositories_Should_PersistAllWrites_When_NoExplicitSaveIsCalled(bool role)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var provider = scope.ServiceProvider;
        var id = Guid.CreateVersion7();

        await AddAsync(provider, role, id, cancellationToken);

        (await ReadNameAsync(role, id, cancellationToken)).ShouldBe("original");
        provider.GetRequiredService<IAggregateTracker>().GetAll().ShouldHaveSingleItem();

        var update = await UpdateAsync(provider, role, id, "updated", cancellationToken);
        update.IsSuccess.ShouldBeTrue();
        (await ReadNameAsync(role, id, cancellationToken)).ShouldBe("updated");

        var deletion = role
            ? await provider.GetRequiredService<IRolesRepository>().DeleteAsync(
                id: id,
                currentAccessToken: null,
                cancellationToken: cancellationToken)
            : await provider.GetRequiredService<IUsersRepository>().DeleteAsync(id, cancellationToken);
        deletion.IsSuccess.ShouldBeTrue();
        (await ReadNameAsync(role, id, cancellationToken)).ShouldBeNull();
    }

    [Theory(DisplayName = "Identity managers should persist claims when no explicit save is called")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AddClaimAsync_Should_PersistClaim_When_NoExplicitSaveIsCalled(bool role)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var provider = scope.ServiceProvider;
        var context = provider.GetRequiredService<IdentityWriteDbContext>();
        var id = Guid.CreateVersion7();
        await AddAsync(provider, role, id, cancellationToken);
        var claim = new Claim("permission", "heroes.read");

        var result = role
            ? await provider.GetRequiredService<RoleManager<ApplicationRole>>()
                .AddClaimAsync(context.Set<ApplicationRole>().Local.Single(item => item.Id == id), claim)
            : await provider.GetRequiredService<UserManager<ApplicationUser>>()
                .AddClaimAsync(context.Set<ApplicationUser>().Local.Single(item => item.Id == id), claim);
        result.Succeeded.ShouldBeTrue();
        (await ReadNameAsync(role, id, cancellationToken)).ShouldBe("original");

        await using var verification = Fixture.CreateScope();
        var verificationContext = verification.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var persistedClaim = role
            ? await verificationContext.Set<ApplicationRoleClaim>()
                .Where(item => item.RoleId == id)
                .Select(item => item.ClaimValue)
                .SingleAsync(cancellationToken)
            : await verificationContext.Set<ApplicationUserClaim>()
                .Where(item => item.UserId == id)
                .Select(item => item.ClaimValue)
                .SingleAsync(cancellationToken);
        persistedClaim.ShouldBe(claim.Value);
    }

    [Theory(DisplayName = "Repositories should persist successive updates when no explicit save is called")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateAsync_Should_PersistSuccessiveUpdates_When_NoExplicitSaveIsCalled(bool role)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var id = Guid.CreateVersion7();
        await AddAsync(scope.ServiceProvider, role, id, cancellationToken);
        var first = await UpdateAsync(scope.ServiceProvider, role, id, "first", cancellationToken);
        first.IsSuccess.ShouldBeTrue();
        (await ReadNameAsync(role, id, cancellationToken)).ShouldBe("first");

        var second = await UpdateAsync(scope.ServiceProvider, role, id, "second", cancellationToken);
        second.IsSuccess.ShouldBeTrue();
        (await ReadNameAsync(role, id, cancellationToken)).ShouldBe("second");
    }

    [Theory(DisplayName = "Repositories should return conflict when another scope has saved")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateAsync_Should_ReturnConflict_When_AnotherScopeHasSaved(bool role)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var id = Guid.CreateVersion7();
        await using var first = Fixture.CreateScope();
        await using var second = Fixture.CreateScope();
        await AddAsync(first.ServiceProvider, role, id, cancellationToken);
        var secondContext = second.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        if (role)
        {
            await secondContext.Set<ApplicationRole>().SingleAsync(item => item.Id == id, cancellationToken);
        }
        else
        {
            await secondContext.Set<ApplicationUser>().SingleAsync(item => item.Id == id, cancellationToken);
        }

        var winner = await UpdateAsync(first.ServiceProvider, role, id, "winner", cancellationToken);
        winner.IsSuccess.ShouldBeTrue();
        var stale = await UpdateAsync(second.ServiceProvider, role, id, "stale", cancellationToken);

        stale.IsFailure.ShouldBeTrue();
        stale.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Conflict);
        (await ReadNameAsync(role, id, cancellationToken)).ShouldBe("winner");
    }

    [Theory(DisplayName = "RollbackTransactionAsync should discard saved changes when transaction is rolled back")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RollbackTransactionAsync_Should_DiscardSavedChanges_When_TransactionIsRolledBack(bool role)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var unitOfWork = scope.ServiceProvider
            .GetRequiredKeyedService<IUnitOfWork>(typeof(IdentityWriteDbContext));
        var id = Guid.CreateVersion7();
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        await AddAsync(scope.ServiceProvider, role, id, cancellationToken);

        await unitOfWork.RollbackTransactionAsync(cancellationToken);

        (await ReadNameAsync(role, id, cancellationToken)).ShouldBeNull();
    }

    private static async Task AddAsync(
        IServiceProvider provider,
        bool role,
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = role
            ? await provider.GetRequiredService<IRolesRepository>().AddAsync(
                RoleTestData.CreateRole(id, "original", []), cancellationToken)
            : await provider.GetRequiredService<IUsersRepository>().AddAsync(
                UserTestData.CreateUser(id, "hero@example.com", true, [], [], "original", UserStatus.Active),
                "StrongPassword1!",
                cancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    private static async Task<Result> UpdateAsync(
        IServiceProvider provider,
        bool role,
        Guid id,
        string name,
        CancellationToken cancellationToken)
    {
        return role
            ? await provider.GetRequiredService<IRolesRepository>().UpdateAsync(
                role: RoleTestData.CreateRole(id, name, []),
                currentAccessToken: null,
                cancellationToken: cancellationToken)
            : await provider.GetRequiredService<IUsersRepository>().UpdateAsync(
                user: UserTestData.CreateUser(id, "hero@example.com", true, [], [], name, UserStatus.Active),
                currentAccessToken: null,
                cancellationToken: cancellationToken);
    }

    private async Task<string?> ReadNameAsync(bool role, Guid id, CancellationToken cancellationToken)
    {
        await using var scope = Fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        return role
            ? await context.Set<ApplicationRole>()
                .Where(item => item.Id == id)
                .Select(item => item.Name)
                .SingleOrDefaultAsync(cancellationToken)
            : await context.Set<ApplicationUser>()
                .Where(item => item.Id == id)
                .Select(item => item.UserName)
                .SingleOrDefaultAsync(cancellationToken);
    }
}
