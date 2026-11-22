using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence.Errors;

using PANiXiDA.TacticalHeroes.Builds.Domain.Builds;
using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.Abstractions;
using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.ValueObjects;
using PANiXiDA.TacticalHeroes.Builds.Domain.Heroes;
using PANiXiDA.TacticalHeroes.Builds.Domain.Units;
using PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Features.Builds.Read.DbModels;
using PANiXiDA.TacticalHeroes.Builds.IntegrationTests.Builds;

namespace PANiXiDA.TacticalHeroes.Builds.IntegrationTests.Infrastructure.Persistence.Features.Builds.Write;

public sealed class BuildsRepositoryTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Build should restore empty selections when a new build is saved")]
    public async Task GetByIdAsync_Should_RestoreEmptySelections_When_ANewBuildIsSaved()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var build = BuildTestData.Create();
        await AddBuildAsync(build, cancellationToken);

        await using var scope = Fixture.CreateScope();
        var restored = await scope.ServiceProvider.GetRequiredService<IBuildsRepository>()
            .GetByIdAsync(build.Id, cancellationToken);

        restored.ShouldNotBeNull();
        restored.Id.ShouldBe(build.Id);
        restored.PlayerId.ShouldBe(build.PlayerId);
        restored.Name.ShouldBe(build.Name);
        restored.Heroes.ShouldBeEmpty();
        restored.Units.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Build should restore both collections when selections exist")]
    public async Task GetByIdAsync_Should_RestoreBothCollections_When_SelectionsExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var build = BuildTestData.Create();
        build.AddHero(HeroId.New());
        build.AddHero(HeroId.New());
        build.AddUnit(UnitId.New());
        build.AddUnit(UnitId.New());
        await AddBuildAsync(build, cancellationToken);

        await using var scope = Fixture.CreateScope();
        var restored = await scope.ServiceProvider.GetRequiredService<IBuildsRepository>()
            .GetByIdAsync(build.Id, cancellationToken);

        restored.ShouldNotBeNull();
        restored.Heroes.Count.ShouldBe(2);
        restored.Units.Count.ShouldBe(2);
        foreach (var expected in build.Heroes)
        {
            var actual = restored.Heroes.Single(hero => hero.Id == expected.Id);
            actual.BuildId.ShouldBe(build.Id);
            actual.HeroId.ShouldBe(expected.HeroId);
        }

        foreach (var expected in build.Units)
        {
            var actual = restored.Units.Single(unit => unit.Id == expected.Id);
            actual.BuildId.ShouldBe(build.Id);
            actual.UnitId.ShouldBe(expected.UnitId);
        }
    }

    [Fact(DisplayName = "Build persistence should allow multiple builds when owner already has a build")]
    public async Task Add_Should_AllowMultipleBuilds_When_OwnerAlreadyHasABuild()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var first = BuildTestData.Create();
        var second = Build.Create(first.PlayerId, first.Name);

        await AddBuildAsync(first, cancellationToken);
        await AddBuildAsync(second, cancellationToken);

        await using var scope = Fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BuildsReadDbContext>();
        var count = await context.Set<BuildReadDbModel>()
            .CountAsync(build => build.PlayerId == first.PlayerId.Value, cancellationToken);

        count.ShouldBe(2);
    }

    [Fact(DisplayName = "Build should persist replacements and remove old entries when selections are edited")]
    public async Task Update_Should_PersistReplacements_When_SelectionsAreEdited()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var build = BuildTestData.Create();
        var oldHero = HeroId.New();
        var oldUnit = UnitId.New();
        var newHero = HeroId.New();
        var newUnit = UnitId.New();
        build.AddHero(oldHero);
        build.AddUnit(oldUnit);
        var oldHeroEntry = build.Heroes.Single().Id.Value;
        var oldUnitEntry = build.Units.Single().Id.Value;
        await AddBuildAsync(build, cancellationToken);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IBuildsRepository>();
            var loaded = (await repository.GetByIdAsync(build.Id, cancellationToken))!;
            loaded.Rename(BuildName.Create("  Updated army  ").Value);
            loaded.RemoveHero(oldHero);
            loaded.RemoveUnit(oldUnit);
            loaded.AddHero(newHero).IsSuccess.ShouldBeTrue();
            loaded.AddUnit(newUnit).IsSuccess.ShouldBeTrue();

            repository.Update(loaded);
            await GetUnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var restored = await verificationScope.ServiceProvider.GetRequiredService<IBuildsRepository>()
            .GetByIdAsync(build.Id, cancellationToken);
        var context = verificationScope.ServiceProvider.GetRequiredService<BuildsReadDbContext>();
        var readModel = await context.Set<BuildReadDbModel>()
            .Include(item => item.Heroes)
            .Include(item => item.Units)
            .AsSplitQuery()
            .SingleAsync(item => item.Id == build.Id.Value, cancellationToken);

        restored.ShouldNotBeNull();
        restored.PlayerId.ShouldBe(build.PlayerId);
        restored.Name.Value.ShouldBe("Updated army");
        restored.Heroes.ShouldHaveSingleItem().HeroId.ShouldBe(newHero);
        restored.Units.ShouldHaveSingleItem().UnitId.ShouldBe(newUnit);
        readModel.Name.ShouldBe("Updated army");
        readModel.Heroes.ShouldHaveSingleItem().HeroId.ShouldBe(newHero.Value);
        readModel.Units.ShouldHaveSingleItem().UnitId.ShouldBe(newUnit.Value);
        var oldHeroExists = await context.Set<BuildHeroReadDbModel>()
            .AnyAsync(item => item.Id == oldHeroEntry, cancellationToken);
        var oldUnitExists = await context.Set<BuildUnitReadDbModel>()
            .AnyAsync(item => item.Id == oldUnitEntry, cancellationToken);

        oldHeroExists.ShouldBeFalse();
        oldUnitExists.ShouldBeFalse();
    }

    [Fact(DisplayName = "Build should reject stale edits when another collection change was saved")]
    public async Task Update_Should_RejectStaleEdits_When_AnotherCollectionChangeWasSaved()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var build = BuildTestData.Create();
        var heroId = HeroId.New();
        await AddBuildAsync(build, cancellationToken);
        await using var firstScope = Fixture.CreateScope();
        await using var secondScope = Fixture.CreateScope();
        var firstRepository = firstScope.ServiceProvider.GetRequiredService<IBuildsRepository>();
        var secondRepository = secondScope.ServiceProvider.GetRequiredService<IBuildsRepository>();
        var first = (await firstRepository.GetByIdAsync(build.Id, cancellationToken))!;
        var second = (await secondRepository.GetByIdAsync(build.Id, cancellationToken))!;
        first.AddHero(heroId);
        firstRepository.Update(first);
        await GetUnitOfWork(firstScope.ServiceProvider).SaveChangesAsync(cancellationToken);
        second.AddUnit(UnitId.New());
        secondRepository.Update(second);

        var exception = await Should.ThrowAsync<ConcurrencyException>(
            () => GetUnitOfWork(secondScope.ServiceProvider).SaveChangesAsync(cancellationToken));

        exception.InnerException.ShouldBeOfType<DbUpdateConcurrencyException>();
        await using var verificationScope = Fixture.CreateScope();
        var restored = await verificationScope.ServiceProvider.GetRequiredService<IBuildsRepository>()
            .GetByIdAsync(build.Id, cancellationToken);
        restored.ShouldNotBeNull();
        restored.Heroes.ShouldHaveSingleItem().HeroId.ShouldBe(heroId);
        restored.Units.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Build should retain its original selections when the transaction is rolled back")]
    public async Task Update_Should_RetainOriginalSelections_When_TheTransactionIsRolledBack()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var build = BuildTestData.Create();
        var heroId = HeroId.New();
        build.AddHero(heroId);
        await AddBuildAsync(build, cancellationToken);
        await using (var scope = Fixture.CreateScope())
        {
            var unitOfWork = GetUnitOfWork(scope.ServiceProvider);
            await unitOfWork.BeginTransactionAsync(cancellationToken);
            var repository = scope.ServiceProvider.GetRequiredService<IBuildsRepository>();
            var loaded = (await repository.GetByIdAsync(build.Id, cancellationToken))!;
            loaded.RemoveHero(heroId);
            loaded.AddUnit(UnitId.New());
            repository.Update(loaded);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            await unitOfWork.DisposeTransactionAsync();
        }

        await using var verificationScope = Fixture.CreateScope();
        var restored = await verificationScope.ServiceProvider.GetRequiredService<IBuildsRepository>()
            .GetByIdAsync(build.Id, cancellationToken);

        restored.ShouldNotBeNull();
        restored.Heroes.ShouldHaveSingleItem().HeroId.ShouldBe(heroId);
        restored.Units.ShouldBeEmpty();
    }

    private static IUnitOfWork GetUnitOfWork(IServiceProvider services)
    {
        return services.GetRequiredKeyedService<IUnitOfWork>(typeof(BuildsWriteDbContext));
    }

    private async Task AddBuildAsync(Build build, CancellationToken cancellationToken)
    {
        await using var scope = Fixture.CreateScope();
        scope.ServiceProvider.GetRequiredService<IBuildsRepository>().Add(build);

        await GetUnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);
    }
}
