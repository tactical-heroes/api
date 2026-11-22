using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Builds.Application.Builds.Abstractions;
using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.Abstractions;
using PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Builds.IntegrationTests.Builds;

namespace PANiXiDA.TacticalHeroes.Builds.IntegrationTests.Infrastructure.Persistence.Features.Builds.Read;

public sealed class BuildsReadRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Read repository should return build when build exists")]
    public async Task GetByIdAsync_Should_ReturnBuild_When_BuildExists()
    {
        var ct = TestContext.Current.CancellationToken;
        var build = BuildTestData.Create();
        await using (var writeScope = Fixture.CreateScope())
        {
            var repository = writeScope.ServiceProvider.GetRequiredService<IBuildsRepository>();
            repository.Add(BuildTestData.Create());
            repository.Add(build);
            await writeScope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(BuildsWriteDbContext))
                .SaveChangesAsync(ct);
        }

        await using var scope = Fixture.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<IBuildsReadRepository>()
            .GetByIdAsync(build.Id.Value, ct);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(build.Id.Value);
        result.PlayerId.ShouldBe(build.PlayerId.Value);
    }

    [Fact(DisplayName = "Read repository should return null when build is missing")]
    public async Task GetByIdAsync_Should_ReturnNull_When_BuildIsMissing()
    {
        await using var scope = Fixture.CreateScope();

        var result = await scope.ServiceProvider.GetRequiredService<IBuildsReadRepository>()
            .GetByIdAsync(Guid.CreateVersion7(), TestContext.Current.CancellationToken);

        result.ShouldBeNull();
    }

    [Fact(DisplayName = "Read repository should return null when build was deleted")]
    public async Task GetByIdAsync_Should_ReturnNull_When_BuildWasDeleted()
    {
        var ct = TestContext.Current.CancellationToken;
        var build = BuildTestData.Create();
        await using (var writeScope = Fixture.CreateScope())
        {
            var repository = writeScope.ServiceProvider.GetRequiredService<IBuildsRepository>();
            var unitOfWork = writeScope.ServiceProvider
                .GetRequiredKeyedService<IUnitOfWork>(typeof(BuildsWriteDbContext));
            repository.Add(build);
            await unitOfWork.SaveChangesAsync(ct);
            repository.Delete(build);
            await unitOfWork.SaveChangesAsync(ct);
        }

        await using var scope = Fixture.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<IBuildsReadRepository>()
            .GetByIdAsync(build.Id.Value, ct);

        result.ShouldBeNull();
    }

    [Fact(DisplayName = "Read repository should report persisted builds when builds exist")]
    public async Task ExistsByIdAsync_Should_ReportPersistedBuilds_When_BuildsExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var build = BuildTestData.Create();
        await using (var scope = Fixture.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IBuildsRepository>().Add(build);
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(BuildsWriteDbContext))
                .SaveChangesAsync(cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var repository = verificationScope.ServiceProvider.GetRequiredService<IBuildsReadRepository>();

        (await repository.ExistsByIdAsync(build.Id.Value, cancellationToken)).ShouldBeTrue();
        (await repository.ExistsByIdAsync(Guid.CreateVersion7(), cancellationToken)).ShouldBeFalse();
    }

    [Fact(DisplayName = "Read repository should detect builds when the first build is saved")]
    public async Task AnyAsync_Should_DetectBuilds_When_TheFirstBuildIsSaved()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IBuildsReadRepository>();
            (await repository.AnyAsync(cancellationToken)).ShouldBeFalse();
        }

        var build = BuildTestData.Create();
        await using (var scope = Fixture.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IBuildsRepository>().Add(build);
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(BuildsWriteDbContext))
                .SaveChangesAsync(cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var populatedRepository = verificationScope.ServiceProvider.GetRequiredService<IBuildsReadRepository>();

        (await populatedRepository.AnyAsync(cancellationToken)).ShouldBeTrue();
    }
}
