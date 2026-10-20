using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence.Errors;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

using RoleTestData = PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Roles.IntegrationTestData;
using UserTestData = PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Users.IntegrationTestData;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence;

public sealed class IdentityUnitOfWorkTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Theory(DisplayName = "Repositories should defer all writes when unit of work has not saved")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Repositories_Should_DeferAllWrites_When_UnitOfWorkHasNotSaved(bool role)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var provider = scope.ServiceProvider;
        var unitOfWork = provider.GetRequiredKeyedService<IUnitOfWork>(typeof(IdentityWriteDbContext));
        var id = Guid.CreateVersion7();

        await AddAsync(provider, role, id, cancellationToken);

        (await ReadNameAsync(role, id, cancellationToken)).ShouldBeNull();
        provider.GetRequiredService<IAggregateTracker>().GetAll().ShouldHaveSingleItem();

        await unitOfWork.SaveChangesAsync(cancellationToken);
        (await ReadNameAsync(role, id, cancellationToken)).ShouldBe("original");

        await UpdateAsync(provider, role, id, "updated", cancellationToken);
        (await ReadNameAsync(role, id, cancellationToken)).ShouldBe("original");

        await unitOfWork.SaveChangesAsync(cancellationToken);
        (await ReadNameAsync(role, id, cancellationToken)).ShouldBe("updated");

        var deletion = role
            ? await provider.GetRequiredService<IRolesRepository>().DeleteAsync(id, cancellationToken)
            : await provider.GetRequiredService<IUsersRepository>().DeleteAsync(id, cancellationToken);
        deletion.IsSuccess.ShouldBeTrue();
        (await ReadNameAsync(role, id, cancellationToken)).ShouldBe("updated");

        await unitOfWork.SaveChangesAsync(cancellationToken);
        (await ReadNameAsync(role, id, cancellationToken)).ShouldBeNull();
    }

    [Theory(DisplayName = "SaveChangesAsync should persist the new entity when a claim is added before saving")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveChangesAsync_Should_PersistNewEntity_When_AClaimIsAddedBeforeSaving(bool role)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var provider = scope.ServiceProvider;
        var context = provider.GetRequiredService<IdentityWriteDbContext>();
        var unitOfWork = provider.GetRequiredKeyedService<IUnitOfWork>(typeof(IdentityWriteDbContext));
        var id = Guid.CreateVersion7();
        await AddAsync(provider, role, id, cancellationToken);
        var claim = new Claim("permission", "heroes.read");

        var result = role
            ? await provider.GetRequiredService<RoleManager<ApplicationRole>>()
                .AddClaimAsync(context.Set<ApplicationRole>().Local.Single(item => item.Id == id), claim)
            : await provider.GetRequiredService<UserManager<ApplicationUser>>()
                .AddClaimAsync(context.Set<ApplicationUser>().Local.Single(item => item.Id == id), claim);
        result.Succeeded.ShouldBeTrue();
        (await ReadNameAsync(role, id, cancellationToken)).ShouldBeNull();
        await unitOfWork.SaveChangesAsync(cancellationToken);

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

    [Theory(DisplayName = "SaveChangesAsync should preserve the original version when entity is updated twice")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveChangesAsync_Should_PreserveOriginalVersion_When_EntityIsUpdatedTwice(bool role)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var unitOfWork = scope.ServiceProvider
            .GetRequiredKeyedService<IUnitOfWork>(typeof(IdentityWriteDbContext));
        var id = Guid.CreateVersion7();
        await AddAsync(scope.ServiceProvider, role, id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await UpdateAsync(scope.ServiceProvider, role, id, "first", cancellationToken);
        await UpdateAsync(scope.ServiceProvider, role, id, "second", cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        (await ReadNameAsync(role, id, cancellationToken)).ShouldBe("second");
    }

    [Theory(DisplayName = "SaveChangesAsync should reject stale changes when another scope has saved")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveChangesAsync_Should_RejectStaleChanges_When_AnotherScopeHasSaved(bool role)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var id = Guid.CreateVersion7();
        await using var first = Fixture.CreateScope();
        await using var second = Fixture.CreateScope();
        var firstUnitOfWork = first.ServiceProvider
            .GetRequiredKeyedService<IUnitOfWork>(typeof(IdentityWriteDbContext));
        var secondUnitOfWork = second.ServiceProvider
            .GetRequiredKeyedService<IUnitOfWork>(typeof(IdentityWriteDbContext));
        await AddAsync(first.ServiceProvider, role, id, cancellationToken);
        await firstUnitOfWork.SaveChangesAsync(cancellationToken);

        await UpdateAsync(first.ServiceProvider, role, id, "winner", cancellationToken);
        await UpdateAsync(second.ServiceProvider, role, id, "stale", cancellationToken);
        await firstUnitOfWork.SaveChangesAsync(cancellationToken);

        var exception = await Should.ThrowAsync<ConcurrencyException>(
            () => secondUnitOfWork.SaveChangesAsync(cancellationToken));

        exception.InnerException.ShouldBeOfType<DbUpdateConcurrencyException>();
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
        await unitOfWork.SaveChangesAsync(cancellationToken);

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

    private static async Task UpdateAsync(
        IServiceProvider provider,
        bool role,
        Guid id,
        string name,
        CancellationToken cancellationToken)
    {
        var result = role
            ? await provider.GetRequiredService<IRolesRepository>().UpdateAsync(
                RoleTestData.CreateRole(id, name, []), cancellationToken)
            : await provider.GetRequiredService<IUsersRepository>().UpdateAsync(
                UserTestData.CreateUser(id, "hero@example.com", true, [], [], name, UserStatus.Active),
                cancellationToken);
        result.IsSuccess.ShouldBeTrue();
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
