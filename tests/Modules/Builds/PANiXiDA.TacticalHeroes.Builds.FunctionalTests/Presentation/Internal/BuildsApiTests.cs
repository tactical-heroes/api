using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence;

using PANiXiDA.TacticalHeroes.Builds.Contracts.Builds;
using PANiXiDA.TacticalHeroes.Builds.Domain.Builds;
using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.Abstractions;
using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.ValueObjects;
using PANiXiDA.TacticalHeroes.Builds.Domain.Players;
using PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Builds.FunctionalTests.Presentation.Internal;

[Collection(FunctionalTestCollectionDefinition.Name)]
public sealed class BuildsApiTests(FunctionalTestFixture fixture)
{
    [Fact(DisplayName = "Module API should return persisted build when build exists")]
    public async Task GetByIdAsync_Should_ReturnPersistedBuild_When_BuildExists()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var build = await SeedBuildAsync(deleted: false, ct);

        await using var scope = fixture.Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<IBuildsApi>()
            .GetByIdAsync(build.Id.Value, ct);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(build.Id.Value);
        result.PlayerId.ShouldBe(build.PlayerId.Value);
    }

    [Fact(DisplayName = "Module API should return null when build is missing")]
    public async Task GetByIdAsync_Should_ReturnNull_When_BuildIsMissing()
    {
        await using var scope = fixture.Services.CreateAsyncScope();

        var result = await scope.ServiceProvider.GetRequiredService<IBuildsApi>()
            .GetByIdAsync(Guid.CreateVersion7(), TestContext.Current.CancellationToken);

        result.ShouldBeNull();
    }

    [Fact(DisplayName = "Module API should return null when build was deleted")]
    public async Task GetByIdAsync_Should_ReturnNull_When_BuildWasDeleted()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var build = await SeedBuildAsync(deleted: true, ct);

        await using var scope = fixture.Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<IBuildsApi>()
            .GetByIdAsync(build.Id.Value, ct);

        result.ShouldBeNull();
    }

    [Fact(DisplayName = "Module API should return null when build id is empty")]
    public async Task GetByIdAsync_Should_ReturnNull_When_BuildIdIsEmpty()
    {
        await using var scope = fixture.Services.CreateAsyncScope();

        var result = await scope.ServiceProvider.GetRequiredService<IBuildsApi>()
            .GetByIdAsync(Guid.Empty, TestContext.Current.CancellationToken);

        result.ShouldBeNull();
    }

    private async Task<Build> SeedBuildAsync(bool deleted, CancellationToken ct)
    {
        var build = Build.Create(PlayerId.New(), BuildName.Create("Internal build").Value);
        await using var scope = fixture.Services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBuildsRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(BuildsWriteDbContext));
        repository.Add(build);
        await unitOfWork.SaveChangesAsync(ct);
        if (deleted)
        {
            repository.Delete(build);
            await unitOfWork.SaveChangesAsync(ct);
        }

        return build;
    }
}
